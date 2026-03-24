using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IInvoice;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePending
{
    public sealed class GetInvoicePendingQueryHandler
        : IRequestHandler<GetInvoicePendingQuery, (List<GetInvoicePendingDto> Items, int TotalCount)>
    {
        private readonly IInvoiceQueryRepository _repo;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IItemLookup _itemLookup;
        private readonly IUOMLookup _uomLookup;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetInvoicePendingQueryHandler(
            IInvoiceQueryRepository repo,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IItemLookup itemLookup,
            IUOMLookup uomLookup,
            IFinancialYearLookup financialYearLookup,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _itemLookup = itemLookup;
            _uomLookup = uomLookup;
            _financialYearLookup = financialYearLookup;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<GetInvoicePendingDto> Items, int TotalCount)> Handle(
            GetInvoicePendingQuery request, CancellationToken ct)
        {
            // 1 — Fetch pending invoices (header + details already grouped by repo)
            var (pending, total) = await _repo.GetInvoicePendingAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
            }

            // 2 — Workflow filtering: only show invoices where current user is the approver
            var currentUserId = _ipAddressService.GetUserId();

            var moduleIds = pending.Select(r => r.Id).Distinct().ToList();
            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeInvoice, moduleIds);

            var allowedIds = wfApprovers
                .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
                            && int.TryParse(a.ApproverValue, out var parsed)
                            && parsed == currentUserId)
                .Select(a => a.ModuleTransactionId)
                .ToHashSet();

            pending = pending.Where(r => allowedIds.Contains(r.Id)).ToList();
            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
            }

            // 3 — Build workflow lookup map
            var wfByModuleId = wfApprovers
                .GroupBy(a => a.ModuleTransactionId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var first = g.First();
                        int? approverId = null;
                        if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
                            int.TryParse(first.ApproverValue, out var a))
                            approverId = a;

                        return new
                        {
                            ApproverId = approverId,
                            ApprovalRequestId = first.ApprovalRequestId,
                            IsEdit = first.IsEdit
                        };
                    });

            // 4 — User names for approvers
            var users = await _userLookup.GetAllUserAsync();
            var userNameMap = users.ToDictionary(u => u.UserId, u => u.UserName ?? string.Empty);

            // 5 — Collect IDs for cross-module enrichment
            var partyIds = pending.Select(r => r.PartyId).Distinct().ToList();
            var agentIds = pending.Where(r => r.AgentId.HasValue).Select(r => r.AgentId!.Value).Distinct().ToList();
            var allPartyIds = partyIds.Union(agentIds).ToList();
            var finYearIds = pending.Select(r => r.FinancialYearId).Distinct().ToList();

            var allDetails = pending
                .SelectMany(r => r.InvoiceDetails ?? new List<GetInvoicePendingDto.GetInvoicePendingDetailDto>())
                .ToList();

            var itemIds = allDetails
                .Select(d => d.ItemId)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            var uomIds = allDetails
                .Where(d => d.UOMId.HasValue)
                .Select(d => d.UOMId!.Value)
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            // 6 — Parallel cross-module lookups
            var partyTask = _partyLookup.GetByIdsAsync(allPartyIds, ct);
            var unitTask = _unitLookup.GetAllUnitAsync();
            var finYearTask = _financialYearLookup.GetByIdsAsync(finYearIds, ct);
            var itemTask = itemIds.Count > 0
                ? _itemLookup.GetByIdsAsync(itemIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.ItemLookupDto>());
            var uomTask = uomIds.Count > 0
                ? _uomLookup.GetByIdsAsync(uomIds, ct)
                : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>>(
                    Array.Empty<Contracts.Dtos.Lookups.Inventory.UOMLookupDto>());

            await Task.WhenAll(partyTask, unitTask, finYearTask, itemTask, uomTask);

            // 7 — Build lookup dictionaries
            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var unitDict = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);
            var finYearDict = (await finYearTask).ToDictionary(f => f.FinancialYearId, f => f.FinancialYearName);

            var itemNameMap = (await itemTask)
                .GroupBy(x => x.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var it = g.First();
                    return !string.IsNullOrWhiteSpace(it.ItemName) ? it.ItemName : (it.ItemCode ?? string.Empty);
                });

            var uomMap = (await uomTask)
                .GroupBy(u => u.Id)
                .ToDictionary(g => g.Key, g =>
                {
                    var u = g.First();
                    return !string.IsNullOrWhiteSpace(u.UOMName) ? u.UOMName : (u.Code ?? u.Id.ToString());
                });

            // 8 — Enrich
            foreach (var r in pending)
            {
                if (r.InvoiceDetails == null) continue;

                // Cross-module header names
                if (partyDict.TryGetValue(r.PartyId, out var partyName))
                    r.PartyName = partyName;

                if (r.AgentId.HasValue && partyDict.TryGetValue(r.AgentId.Value, out var agentName))
                    r.AgentName = agentName;

                if (unitDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;

                if (finYearDict.TryGetValue(r.FinancialYearId, out var finYearName))
                    r.FinancialYearName = finYearName;

                // Detail-level enrichment
                foreach (var detail in r.InvoiceDetails)
                {
                    if (itemNameMap.TryGetValue(detail.ItemId, out var iName))
                        detail.ItemName = iName;

                    if (detail.UOMId.HasValue && uomMap.TryGetValue(detail.UOMId.Value, out var uName))
                        detail.UOMName = uName;
                }

                // Workflow enrichment
                if (wfByModuleId.TryGetValue(r.Id, out var wf))
                {
                    if (wf.ApproverId.HasValue)
                    {
                        r.ApproverId = wf.ApproverId.Value;
                        if (userNameMap.TryGetValue(r.ApproverId, out var approverName))
                            r.ApproverName = approverName;
                    }
                    r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                    r.IsEdit = wf.IsEdit;
                }
            }

            await PublishAudit(pending.Count, request, ct);
            return (pending, pending.Count);
        }

        private Task PublishAudit(int count, GetInvoicePendingQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-Pending",
                    actionCode: string.Empty,
                    actionName: "InvoicePending",
                    details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                    module: "Invoice"), ct);
    }
}
