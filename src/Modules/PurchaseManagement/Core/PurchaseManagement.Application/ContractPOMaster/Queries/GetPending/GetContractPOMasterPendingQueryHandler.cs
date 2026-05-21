using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

public sealed class GetContractPOMasterPendingQueryHandler
    : IRequestHandler<GetContractPOMasterPendingQuery, (List<GetContractPOMasterPendingGroupDto> Items, int TotalCount)>
{
    private readonly IContractPOMasterQueryRepository _repo;
    private readonly IMediator _mediator;
    private readonly IWorkflowLookup _workflowLookup;
    private readonly IUserLookup _userLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly IItemLookup _itemLookup;
    private readonly IPartyLookup _partyLookup;

    public GetContractPOMasterPendingQueryHandler(
        IContractPOMasterQueryRepository repo,
        IMediator mediator,
        IWorkflowLookup workflowLookup,
        IUserLookup userLookup,
        IIPAddressService ipAddressService,
        IItemLookup itemLookup,
        IPartyLookup partyLookup)
    {
        _repo = repo;
        _mediator = mediator;
        _workflowLookup = workflowLookup;
        _userLookup = userLookup;
        _ipAddressService = ipAddressService;
        _itemLookup = itemLookup;
        _partyLookup = partyLookup;
    }

    public async Task<(List<GetContractPOMasterPendingGroupDto> Items, int TotalCount)> Handle(
        GetContractPOMasterPendingQuery request, CancellationToken ct)
    {
        var (rows, _) = await _repo.GetContractPOMasterPendingAsync(
            request.PageNumber ?? 1, request.PageSize ?? 15, request.SearchTerm, ct);

        if (rows.Count == 0)
        {
            await PublishAudit(0, request, ct);
            return (rows, 0);
        }

        // ---------------- Filter by current user's approver id ----------------
        var currentUserId = _ipAddressService.GetUserId();

        var contractIds = rows.Select(r => r.Id)
                              .Where(id => id > 0)
                              .Distinct()
                              .ToList();

        var wfApprovers = await _workflowLookup
            .GetApproverListAsync(MiscEnumEntity.TransactionTypeContract.ToString(), contractIds);

        // Allowed IDs where current user is an approver
        var allowedIds = wfApprovers
            .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                        && int.TryParse(a.ApproverValue, out var parsed)
                        && parsed == currentUserId)
            .Select(a => a.ModuleTransactionId)
            .ToHashSet();

        rows = rows.Where(r => allowedIds.Contains(r.Id)).ToList();

        if (rows.Count == 0)
        {
            await PublishAudit(0, request, ct);
            return (rows, 0);
        }

        // Build workflow map for kept records
        var wfByModuleId = wfApprovers
            .Where(a => allowedIds.Contains(a.ModuleTransactionId))
            .GroupBy(a => a.ModuleTransactionId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.FirstOrDefault(x => int.TryParse(x.ApproverValue, out _)) ?? g.First();
                    int? approverId = null;
                    if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                        int.TryParse(first.ApproverValue, out var parsed))
                    {
                        approverId = parsed;
                    }
                    return new
                    {
                        ApproverId = approverId,
                        ApprovalRequestId = first.ApprovalRequestId,
                        IsEdit = first.IsEdit
                    };
                });

        // Users -> names
        var users = await _userLookup.GetAllUserAsync();
        var userLookupDict = users.ToDictionary(u => u.UserId, u => u.UserName);

        // Ensure Lines non-null
        foreach (var g in rows)
            g.Lines ??= new List<GetContractPOMasterPendingDto>();

        // Collect IDs (from filtered rows only)
        var allLines = rows.SelectMany(g => g.Lines).ToList();
        var itemIds = allLines.Select(l => l.ItemId).Where(id => id > 0).Distinct().ToList();
        var vendorIds = rows.Select(r => r.VendorId).Where(v => v > 0).Distinct().ToList();

        // Parallel enrichment calls
        var itemsTask = _itemLookup.GetByIdsAsync(itemIds, ct);
        var vendorsTask = _partyLookup.GetByIdsAsync(vendorIds, ct);
        await Task.WhenAll(itemsTask, vendorsTask);

        // Build vendor map
        var vendorMap = (await vendorsTask)
            .Where(p => p != null)
            .ToDictionary(
                p => p.Id,
                p => (Name: p.PartyName ?? string.Empty, Email: p.Email, Mobile: p.Mobile));

        // Items map
        var itemNameMap = (await itemsTask)
            .GroupBy(x => x.Id)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var it = g.First();
                    return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName : (it.ItemCode ?? string.Empty);
                });

        // ---------------- Enrich results ----------------
        foreach (var g in rows)
        {
            foreach (var line in g.Lines)
            {
                if (itemNameMap.TryGetValue(line.ItemId, out var itemName))
                    line.ItemName = itemName;
            }

            // Workflow enrichment
            if (wfByModuleId.TryGetValue(g.Id, out var wf))
            {
                if (wf.ApproverId.HasValue)
                {
                    g.ApproverId = wf.ApproverId.Value;
                    if (userLookupDict.TryGetValue(g.ApproverId.Value, out var approverName))
                        g.ApproverName = approverName;
                }
                g.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                g.IsEdit = wf.IsEdit;
            }

            if (g.VendorId > 0 && vendorMap.TryGetValue(g.VendorId, out var v))
            {
                g.VendorName = v.Name;
                g.VendorEmail = v.Email;
                g.VendorMobile = v.Mobile;
            }
        }

        await PublishAudit(rows.Count, request, ct);
        return (rows, rows.Count);
    }

    private Task PublishAudit(int count, GetContractPOMasterPendingQuery q, CancellationToken ct)
        => _mediator.Publish(new AuditLogsDomainEvent(
            actionDetail: "GetAll-Pending",
            actionCode: string.Empty,
            actionName: "ContractPOMasterPending",
            details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
            module: "ContractPOMaster"), ct);
}
