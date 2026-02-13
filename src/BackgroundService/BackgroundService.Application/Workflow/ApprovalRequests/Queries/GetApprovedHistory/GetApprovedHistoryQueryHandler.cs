using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory
{
    public class GetApprovedHistoryQueryHandler : IRequestHandler<GetApprovedHistoryQuery, List<ApprovedHistoryDto>>
    {
        private readonly IApprovalRequestQuery _approvalRequestQuery;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;

        public GetApprovedHistoryQueryHandler(
            IApprovalRequestQuery approvalRequestQuery,
            IMapper mapper,
            ILookupRepository lookupRepository)
        {
            _approvalRequestQuery = approvalRequestQuery;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
        }

        public async Task<List<ApprovedHistoryDto>> Handle(GetApprovedHistoryQuery request, CancellationToken cancellationToken)
        {
            var approvalHistory = await _approvalRequestQuery.GetApprovedHistory(request.WorkflowType, request.ModuleTransactionId);
            var approvalHistoryDto = _mapper.Map<List<ApprovedHistoryDto>>(approvalHistory);

            // Get user IDs from ApproverValue (parse string to int)
            var userIds = approvalHistoryDto
                .Where(a => int.TryParse(a.ApproverValue, out _))
                .Select(a => int.Parse(a.ApproverValue))
                .Distinct();

            var userLookup = await _lookupRepository.GetUserNamesAsync(userIds, cancellationToken);

            foreach (var approval in approvalHistoryDto)
            {
                if (int.TryParse(approval.ApproverValue, out var userId) &&
                    userLookup.TryGetValue(userId, out var approverName))
                {
                    approval.ApproverName = approverName;
                }
                else
                {
                    approval.ApproverName = "Unknown User";
                }
            }

            return approvalHistoryDto;
        }
    }
}
