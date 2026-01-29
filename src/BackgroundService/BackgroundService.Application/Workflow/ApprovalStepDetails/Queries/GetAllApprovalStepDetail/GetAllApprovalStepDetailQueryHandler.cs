using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using Contracts.Interfaces.External.IUser;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail
{
    public class GetAllApprovalStepDetailQueryHandler : IRequestHandler<GetAllApprovalStepDetailQuery, ApiResponseDTO<List<ApprovalStepDetailDto>>>
    {
         private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMenuGrpcClient _menuGrpcClient;
        public GetAllApprovalStepDetailQueryHandler(IApprovalStepDetailQuery approvalStepDetailQuery, IMediator mediator, IMapper mapper, IMenuGrpcClient menuGrpcClient)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
            _mediator = mediator;
            _mapper = mapper;
            _menuGrpcClient = menuGrpcClient;
        }
        public async Task<ApiResponseDTO<List<ApprovalStepDetailDto>>> Handle(GetAllApprovalStepDetailQuery request, CancellationToken cancellationToken)
        {
             var (ApprovalStepDetail, TotalCount) = await _approvalStepDetailQuery.GetAllApprovalStepDetailAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var ApprovalStepDetailDto = _mapper.Map<List<ApprovalStepDetailDto>>(ApprovalStepDetail);

             var menus = await _menuGrpcClient.GetMenuIdsAsync(ApprovalStepDetailDto.Select(x => x.MenuId).ToList());

            var menuLookup  = menus.ToDictionary(d => d.Id, d => d.MenuName);

            foreach (var dto in ApprovalStepDetailDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var MenuName))
                {
                    dto.MenuName = MenuName;
                }
            }


            return new ApiResponseDTO<List<ApprovalStepDetailDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = ApprovalStepDetailDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}