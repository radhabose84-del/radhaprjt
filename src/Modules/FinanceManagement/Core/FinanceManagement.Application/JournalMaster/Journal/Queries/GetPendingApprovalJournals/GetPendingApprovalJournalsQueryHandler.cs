using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Queries.GetPendingApprovalJournals
{
    public class GetPendingApprovalJournalsQueryHandler : IRequestHandler<GetPendingApprovalJournalsQuery, ApiResponseDTO<List<JournalListItemDto>>>
    {
        private readonly IJournalQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public GetPendingApprovalJournalsQueryHandler(
            IJournalQueryRepository queryRepository,
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

        public async Task<ApiResponseDTO<List<JournalListItemDto>>> Handle(GetPendingApprovalJournalsQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetPendingApprovalAsync(request.PageNumber, request.PageSize, companyId);

            if (data.Count > 0)
            {
                // Only show rows where the current user is the pending workflow approver
                // (mirrors GetPendingTaxAccountLinkageQueryHandler).
                var currentUserId = _ipAddressService.GetUserId();
                var moduleIds = data.Select(r => r.Id).Distinct().ToList();

                var wfApprovers = await _workflowLookup.GetApproverListAsync(MiscEnumEntity.JournalVoucher, moduleIds);

                var allowedIds = wfApprovers
                    .Where(a => int.TryParse(a.ApproverValue, out var uid) && uid == currentUserId)
                    .Select(a => a.ModuleTransactionId)
                    .ToHashSet();

                data = data.Where(r => allowedIds.Contains(r.Id)).ToList();
                totalCount = data.Count;

                // Resolve the pending approver name(s) per row → ApproverName (who needs to approve).
                if (data.Count > 0)
                {
                    var approverIdsByModule = wfApprovers
                        .Where(a => int.TryParse(a.ApproverValue, out _))
                        .GroupBy(a => a.ModuleTransactionId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(a => int.Parse(a.ApproverValue)).Distinct().ToList());

                    var allApproverIds = approverIdsByModule.Values.SelectMany(x => x).Distinct().ToList();
                    IReadOnlyList<UserLookupDto> users = allApproverIds.Count > 0
                        ? await _userLookup.GetByIdsAsync(allApproverIds, cancellationToken)
                        : (IReadOnlyList<UserLookupDto>)Array.Empty<UserLookupDto>();
                    var nameById = users.ToDictionary(u => u.UserId, u => u.UserName);

                    foreach (var row in data)
                    {
                        if (approverIdsByModule.TryGetValue(row.Id, out var ids) && ids.Count > 0
                            && nameById.TryGetValue(ids[0], out var name))
                        {
                            row.ApproverName = name;
                        }
                    }
                }
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingApprovalJournalsQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Journal vouchers pending approval were fetched.",
                module: "Journal"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<JournalListItemDto>>
            {
                IsSuccess = true,
                Message = "Journal vouchers pending approval retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
