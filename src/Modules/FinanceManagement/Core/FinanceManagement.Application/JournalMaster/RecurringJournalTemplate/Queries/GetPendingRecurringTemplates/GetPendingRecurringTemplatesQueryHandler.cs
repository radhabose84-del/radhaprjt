using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetPendingRecurringTemplates
{
    public class GetPendingRecurringTemplatesQueryHandler
        : IRequestHandler<GetPendingRecurringTemplatesQuery, ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>>
    {
        private readonly IRecurringJournalTemplateQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IMediator _mediator;

        public GetPendingRecurringTemplatesQueryHandler(
            IRecurringJournalTemplateQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IWorkflowLookup workflowLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _workflowLookup = workflowLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>> Handle(GetPendingRecurringTemplatesQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await _queryRepository.GetPendingApprovalAsync(request.PageNumber, request.PageSize);

            if (data.Count > 0)
            {
                // Only show templates where the current user is the pending workflow approver (TaxCode/Journal pattern).
                var currentUserId = _ipAddressService.GetUserId();
                var moduleIds = data.Select(r => r.Id).Distinct().ToList();

                var wfApprovers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.RecurringJournalTemplate, moduleIds);
                var allowedIds = wfApprovers
                    .Where(a => int.TryParse(a.ApproverValue, out var uid) && uid == currentUserId)
                    .Select(a => a.ModuleTransactionId)
                    .ToHashSet();

                data = data.Where(r => allowedIds.Contains(r.Id)).ToList();
                totalCount = data.Count;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingRecurringTemplatesQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Recurring templates pending approval were fetched.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<RecurringJournalTemplateHeaderDto>>
            {
                IsSuccess = true,
                Message = "Recurring templates pending approval retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
