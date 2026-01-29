using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRequest;
using Contracts.Interfaces.External.IUser;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Queries.GetApprovedHistory
{
    public class GetApprovedHistoryQueryHandler : IRequestHandler<GetApprovedHistoryQuery, List<ApprovedHistoryDto>>
    {
        private readonly IApprovalRequestQuery _approvalRequestQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IUsersAllGrpcClient _usersAllGrpcClient;
        public GetApprovedHistoryQueryHandler(IApprovalRequestQuery approvalRequestQuery, IMapper mapper, IMediator mediator, IUsersAllGrpcClient usersAllGrpcClient)
        {
            _approvalRequestQuery = approvalRequestQuery;
            _mapper = mapper;
            _mediator = mediator;
            _usersAllGrpcClient = usersAllGrpcClient;
        }
        public async Task<List<ApprovedHistoryDto>> Handle(GetApprovedHistoryQuery request, CancellationToken cancellationToken)
        {
            var approvalHistory = await _approvalRequestQuery.GetApprovedHistory(request.WorkflowType, request.ModuleTransactionId);

            var approvalHistoryDto = _mapper.Map<List<ApprovedHistoryDto>>(approvalHistory);

            var users = await _usersAllGrpcClient.GetUserAllAsync();
            var userDict = users.ToDictionary(u => u.UserId.ToString(), u => u.UserName);

            foreach (var approval in approvalHistoryDto)
            {
                if (userDict.TryGetValue(approval.ApproverValue, out var approverName))
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