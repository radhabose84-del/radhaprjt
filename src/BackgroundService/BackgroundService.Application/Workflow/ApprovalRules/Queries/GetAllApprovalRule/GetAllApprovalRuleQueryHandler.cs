using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Queries.GetAllApprovalRule
{
    public class GetAllApprovalRuleQueryHandler : IRequestHandler<GetAllApprovalRuleQuery, ApiResponseDTO<List<ApprovalRuleDto>>>
    {
        private readonly IApprovalRuleQuery _approvalRuleQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        public GetAllApprovalRuleQueryHandler(IApprovalRuleQuery approvalRuleQuery, IMediator mediator, IMapper mapper,
            ILookupRepository lookupRepository)
        {
            _approvalRuleQuery = approvalRuleQuery;
            _mediator = mediator;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
        }
        public async Task<ApiResponseDTO<List<ApprovalRuleDto>>> Handle(GetAllApprovalRuleQuery request, CancellationToken cancellationToken)
        {
            var (ApprovalRule, TotalCount) = await _approvalRuleQuery.GetAllApprovalRuleAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var ApprovalRuleDto = _mapper.Map<List<ApprovalRuleDto>>(ApprovalRule);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(ApprovalRuleDto.Select(x => x.MenuId), cancellationToken);

            foreach (var dto in ApprovalRuleDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var MenuName))
                {
                    dto.MenuName = MenuName;
                }
            }

            
            //  var units = await _unitGrpcClient.GetAllUnitAsync();
            // var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

            // foreach (var approval in ApprovalRuleDto)
            // {
            //     if (unitDict.TryGetValue(approval.UnitId, out var unitName))
            //     {
            //         approval.UnitName = unitName;
            //     }
            //     else
            //     {
            //         approval.UnitName = "Unknown Unit";
            //     }
            // }
            return new ApiResponseDTO<List<ApprovalRuleDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = ApprovalRuleDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
