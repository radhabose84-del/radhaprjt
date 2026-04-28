using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendmentById
{
    public class GetPendingSalesQuotationAmendmentByIdQueryHandler
        : IRequestHandler<GetPendingSalesQuotationAmendmentByIdQuery, SalesQuotationAmendmentHeaderDto?>
    {
        private readonly ISalesQuotationAmendmentQueryRepository _repo;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public GetPendingSalesQuotationAmendmentByIdQueryHandler(
            ISalesQuotationAmendmentQueryRepository repo,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _repo = repo;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<SalesQuotationAmendmentHeaderDto?> Handle(
            GetPendingSalesQuotationAmendmentByIdQuery request, CancellationToken ct)
        {
            var result = await _repo.GetByIdAsync(request.Id);
            if (result == null)
                return null;

            // Workflow enrichment
            var currentUserId = _ipAddressService.GetUserId();
            var wfApprovers = await _workflowLookup
                .GetApproverListAsync(MiscEnumEntity.TransactionTypeSalesQuotationAmendment, [result.Id]);

            var wfByTransaction = wfApprovers
                .GroupBy(x => x.ModuleTransactionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            int approverId = 0;
            int approvalRequestHeaderId = 0;
            if (wfByTransaction.TryGetValue(result.Id, out var approverEntries))
            {
                var entry = approverEntries
                    .FirstOrDefault(a => a.ApproverValue == currentUserId.ToString())
                    ?? approverEntries.First();

                approvalRequestHeaderId = entry.ApprovalRequestId;
                _ = int.TryParse(entry.ApproverValue, out approverId);
            }

            // Approver name
            var users = await _userLookup.GetAllUserAsync();
            var userNameMap = users.ToDictionary(u => u.UserId, u => u.UserName ?? string.Empty);
            var approverName = userNameMap.TryGetValue(approverId, out var name) ? name : null;

            result.ApprovedBy = approverId;

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetPendingById",
                actionCode: "GetPendingSalesQuotationAmendmentByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Sales Quotation Amendment pending Id {result.Id} fetched.",
                module: "SalesQuotationAmendment"), ct);

            return result;
        }
    }
}
