using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder
{
    public sealed class GetPendingSalesOrderQueryHandler
        : IRequestHandler<GetPendingSalesOrderQuery, (List<PendingSalesOrderDto> Items, int TotalCount)>
    {
        private readonly ISalesOrderQueryRepository _repo;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetPendingSalesOrderQueryHandler(
            ISalesOrderQueryRepository repo,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<PendingSalesOrderDto> Items, int TotalCount)> Handle(
            GetPendingSalesOrderQuery request, CancellationToken ct)
        {
            // 1 — Fetch pending sales orders from repo
            var (pending, total) = await _repo.GetPendingSalesOrderAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
            }

            // 2 — Workflow filtering: only show orders where current user is the approver
            var currentUserId = _ipAddressService.GetUserId();

            var moduleIds = pending.Select(r => r.Id).Distinct().ToList();
            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeSalesOrder, moduleIds);

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

            // 5 — Cross-module lookups
            var partyIds = pending.Select(r => r.PartyId).Distinct().ToList();
            var agentIds = pending.Where(r => r.AgentId.HasValue).Select(r => r.AgentId!.Value).Distinct().ToList();
            var allPartyIds = partyIds.Union(agentIds).ToList();

            var partyTask = _partyLookup.GetByIdsAsync(allPartyIds, ct);
            var unitTask = _unitLookup.GetAllUnitAsync();

            await Task.WhenAll(partyTask, unitTask);

            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var unitDict = (await unitTask).ToDictionary(u => u.UnitId, u => u.UnitName);

            // 6 — Enrich
            foreach (var r in pending)
            {
                if (partyDict.TryGetValue(r.PartyId, out var partyName))
                    r.PartyName = partyName;

                if (r.AgentId.HasValue && partyDict.TryGetValue(r.AgentId.Value, out var agentName))
                    r.AgentName = agentName;

                if (unitDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;

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

        private Task PublishAudit(int count, GetPendingSalesOrderQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-Pending",
                    actionCode: string.Empty,
                    actionName: "SalesOrderPending",
                    details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                    module: "SalesOrder"), ct);
    }
}
