using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using Contracts.Interfaces.External.IUser;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete
{
    public class GetApprovalStepDetailAutoCompleteQueryHandler : IRequestHandler<GetApprovalStepDetailAutoCompleteQuery, List<ApprovalStepDetailAutoCompleteDto>>
    {
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMapper _imapper;
        private readonly IUsersAllGrpcClient _usersAllGrpcClient;
        private readonly IMenuGrpcClient _menuGrpcClient;
        public GetApprovalStepDetailAutoCompleteQueryHandler(IApprovalStepDetailQuery approvalStepDetailQuery, IMapper imapper, IUsersAllGrpcClient usersAllGrpcClient, IMenuGrpcClient menuGrpcClient)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
            _imapper = imapper;
            _usersAllGrpcClient = usersAllGrpcClient;
            _menuGrpcClient = menuGrpcClient;
        }
        public async Task<List<ApprovalStepDetailAutoCompleteDto>> Handle(GetApprovalStepDetailAutoCompleteQuery request, CancellationToken cancellationToken)
        {
              var Result = await _approvalStepDetailQuery.GetApprovalStepDetailAutoComplete(request.SearchPattern ?? string.Empty);

              var ApprovalRule = _imapper.Map<List<ApprovalStepDetailAutoCompleteDto>>(Result);

                var users = await _usersAllGrpcClient.GetUserAllAsync();
            var userDict = users.ToDictionary(u => u.UserId, u => u.UserName);

            foreach (var approvalStep in ApprovalRule)
            {
                if (userDict.TryGetValue(approvalStep.TargetValueId, out var approverName))
                {
                    approvalStep.ApproverName = approverName;
                }
                else
                {
                    approvalStep.ApproverName = "Unknown User";
                }
            }

              var menus = await _menuGrpcClient.GetMenuIdsAsync(ApprovalRule.Select(x => x.MenuId).ToList());

            var menuLookup  = menus.ToDictionary(d => d.Id, d => d.MenuName);

            foreach (var dto in ApprovalRule)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var MenuName))
                {
                    dto.MenuName = MenuName;
                }
            }
            
            
            return ApprovalRule;
        }
    }
}