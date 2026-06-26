using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
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
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public GetPendingRecurringTemplatesQueryHandler(
            IRecurringJournalTemplateQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _workflowLookup = workflowLookup;
            _userLookup = userLookup;
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

                // Enrich each row with the pending workflow metadata → ApproverId / ApproverName /
                // ApprovalRequestHeaderId / IsEdit (mirrors GetPendingApprovalJournalsQueryHandler).
                if (data.Count > 0)
                {
                    var wfByModuleId = wfApprovers
                        .GroupBy(a => a.ModuleTransactionId)
                        .ToDictionary(g => g.Key, g => g.First());

                    var allApproverIds = wfByModuleId.Values
                        .Where(a => int.TryParse(a.ApproverValue, out _))
                        .Select(a => int.Parse(a.ApproverValue))
                        .Distinct()
                        .ToList();
                    IReadOnlyList<UserLookupDto> users = allApproverIds.Count > 0
                        ? await _userLookup.GetByIdsAsync(allApproverIds, cancellationToken)
                        : (IReadOnlyList<UserLookupDto>)Array.Empty<UserLookupDto>();
                    var nameById = users.ToDictionary(u => u.UserId, u => u.UserName);

                    foreach (var row in data)
                    {
                        if (!wfByModuleId.TryGetValue(row.Id, out var wf))
                            continue;

                        row.ApprovalRequestHeaderId = wf.ApprovalRequestId;
                        row.IsEdit = wf.IsEdit;
                        if (int.TryParse(wf.ApproverValue, out var approverId))
                        {
                            row.ApproverId = approverId;
                            if (nameById.TryGetValue(approverId, out var name))
                                row.ApproverName = name;
                        }
                    }
                }
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
