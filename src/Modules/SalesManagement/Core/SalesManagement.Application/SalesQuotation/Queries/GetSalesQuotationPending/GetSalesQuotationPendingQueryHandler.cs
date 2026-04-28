using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationPending
{
    public sealed class GetSalesQuotationPendingQueryHandler
        : IRequestHandler<GetSalesQuotationPendingQuery, (List<GetSalesQuotationPendingDto> Items, int TotalCount)>
    {
        private readonly ISalesQuotationQueryRepository _repo;
        private readonly IPartyLookup _partyLookup;
        private readonly IPaymentTermLookup _paymentTermLookup;
        private readonly IMediator _mediator;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;

        public GetSalesQuotationPendingQueryHandler(
            ISalesQuotationQueryRepository repo,
            IPartyLookup partyLookup,
            IPaymentTermLookup paymentTermLookup,
            IMediator mediator,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService)
        {
            _repo = repo;
            _partyLookup = partyLookup;
            _paymentTermLookup = paymentTermLookup;
            _mediator = mediator;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<GetSalesQuotationPendingDto> Items, int TotalCount)> Handle(
            GetSalesQuotationPendingQuery request, CancellationToken ct)
        {
            // 1 — Fetch pending quotations (header-only)
            var (pending, total) = await _repo.GetSalesQuotationPendingAsync(
                request.PageNumber, request.PageSize, request.SearchTerm);

            if (pending.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (pending, 0);
            }

            // 2 — Workflow filtering: only show quotations where current user is the approver
            var currentUserId = _ipAddressService.GetUserId();

            var moduleIds = pending.Select(r => r.Id).Distinct().ToList();
            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeSalesQuotation, moduleIds);

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
            var customerIds = pending.Select(r => r.CustomerId).Distinct().ToList();

            var partyTask = _partyLookup.GetByIdsAsync(customerIds, ct);
            var paymentTermTask = _paymentTermLookup.GetAllPaymentTermAsync();

            await Task.WhenAll(partyTask, paymentTermTask);

            var partyDict = (await partyTask).ToDictionary(p => p.Id, p => p.PartyName);
            var ptDict = (await paymentTermTask).ToDictionary(p => p.Id, p => p.Description);

            // 6 — Enrich
            foreach (var r in pending)
            {
                // Cross-module header names
                if (partyDict.TryGetValue(r.CustomerId, out var customerName))
                    r.CustomerName = customerName;

                if (ptDict.TryGetValue(r.PaymentTermId, out var ptName))
                    r.PaymentTermDescription = ptName;

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

        private Task PublishAudit(int count, GetSalesQuotationPendingQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                    actionDetail: "GetAll-Pending",
                    actionCode: string.Empty,
                    actionName: "SalesQuotationPending",
                    details: $"Fetched {count} rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                    module: "SalesQuotation"), ct);
    }
}
