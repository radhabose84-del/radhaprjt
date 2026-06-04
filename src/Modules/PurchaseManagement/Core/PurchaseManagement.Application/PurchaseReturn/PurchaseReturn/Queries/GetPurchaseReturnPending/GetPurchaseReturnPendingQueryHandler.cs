using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Queries.GetPurchaseReturnPending;

public sealed class GetPurchaseReturnPendingQueryHandler
    : IRequestHandler<GetPurchaseReturnPendingQuery, (IReadOnlyList<PurchaseReturnPendingDto> Items, int Total)>
{
    private readonly IPurchaseReturnQueryRepository _repo;
    private readonly IWorkflowLookup _workflowLookup;
    private readonly IUserLookup _userLookup;
    private readonly IIPAddressService _ip;
    private readonly IMediator _mediator;

    public GetPurchaseReturnPendingQueryHandler(
        IPurchaseReturnQueryRepository repo,
        IWorkflowLookup workflowLookup,
        IUserLookup userLookup,
        IIPAddressService ip,
        IMediator mediator)
    {
        _repo = repo;
        _workflowLookup = workflowLookup;
        _userLookup = userLookup;
        _ip = ip;
        _mediator = mediator;
    }

    public async Task<(IReadOnlyList<PurchaseReturnPendingDto> Items, int Total)> Handle(
        GetPurchaseReturnPendingQuery request, CancellationToken ct)
    {
        var rows = await _repo.GetPendingAsync(request.PageNumber, request.PageSize, request.SearchTerm, ct);
        if (rows.Count == 0)
            return (rows, 0);

        // Keep only RTVs where the current user is the assigned approver (from AppData.ApprovalRequest).
        var currentUserId = _ip.GetUserId();
        var rtvIds = rows.Select(r => r.Id).Where(id => id > 0).Distinct().ToList();

        var approvers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.RtvModuleTypeName, rtvIds);

        var allowedIds = approvers
            .Where(a => int.TryParse(a.ApproverValue, out var u) && u == currentUserId)
            .Select(a => a.ModuleTransactionId)
            .ToHashSet();

        var filtered = rows.Where(r => allowedIds.Contains(r.Id)).ToList();
        if (filtered.Count == 0)
            return (filtered, 0);

        var wfByRtvId = approvers
            .Where(a => allowedIds.Contains(a.ModuleTransactionId))
            .GroupBy(a => a.ModuleTransactionId)
            .ToDictionary(g => g.Key, g => g.First());

        var users = await _userLookup.GetAllUserAsync();
        var userMap = users.ToDictionary(u => u.UserId, u => u.UserName);

        foreach (var r in filtered)
        {
            if (wfByRtvId.TryGetValue(r.Id, out var wf))
            {
                if (int.TryParse(wf.ApproverValue, out var approverId))
                {
                    r.ApproverId = approverId;
                    if (userMap.TryGetValue(approverId, out var name))
                        r.ApproverName = name;
                }
                r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                r.IsEdit = wf.IsEdit;
            }
        }

        var ev = new AuditLogsDomainEvent(
            actionDetail: "GetAll-Pending",
            actionCode: "GetPurchaseReturnPendingQuery",
            actionName: "PurchaseReturnPending",
            details: $"Fetched {filtered.Count} pending Purchase Returns for approver {currentUserId}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return (filtered, filtered.Count);
    }
}
