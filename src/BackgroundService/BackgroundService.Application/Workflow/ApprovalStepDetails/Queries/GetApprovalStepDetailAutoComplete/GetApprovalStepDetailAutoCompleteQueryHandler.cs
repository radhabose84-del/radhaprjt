using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete
{
    public class GetApprovalStepDetailAutoCompleteQueryHandler : IRequestHandler<GetApprovalStepDetailAutoCompleteQuery, List<ApprovalStepDetailAutoCompleteDto>>
    {
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMapper _imapper;
        private readonly ILookupRepository _lookupRepository;
        public GetApprovalStepDetailAutoCompleteQueryHandler(IApprovalStepDetailQuery approvalStepDetailQuery, IMapper imapper, ILookupRepository lookupRepository)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
            _imapper = imapper;
            _lookupRepository = lookupRepository;
        }
        public async Task<List<ApprovalStepDetailAutoCompleteDto>> Handle(GetApprovalStepDetailAutoCompleteQuery request, CancellationToken cancellationToken)
        {
              var Result = await _approvalStepDetailQuery.GetApprovalStepDetailAutoComplete(request.SearchPattern ?? string.Empty);

              var ApprovalRule = _imapper.Map<List<ApprovalStepDetailAutoCompleteDto>>(Result);

            var menuIdLookupTask = _lookupRepository.GetMenuNamesAsync(
                ApprovalRule.Select(x => x.MenuId),
                cancellationToken);
            var userLookupTask = _lookupRepository.GetUserNamesAsync(
                ApprovalRule.Select(x => x.TargetValueId),
                cancellationToken);

            await Task.WhenAll(userLookupTask, menuIdLookupTask);
            var userLookup = userLookupTask.Result;
            var menuLookup = menuIdLookupTask.Result;

            foreach (var approvalStep in ApprovalRule)
            {
                approvalStep.ApproverName = userLookup.TryGetValue(approvalStep.TargetValueId, out var approverName)
                    ? approverName
                    : "Unknown User";

                if (menuLookup.TryGetValue(approvalStep.MenuId, out var menuName))
                {
                    approvalStep.MenuName = menuName;
                }
            }

            
            return ApprovalRule;
        }
    }
}
