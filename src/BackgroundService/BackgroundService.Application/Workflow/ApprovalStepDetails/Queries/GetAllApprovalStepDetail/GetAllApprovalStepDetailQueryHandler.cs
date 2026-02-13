using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetAllApprovalStepDetail
{
    public class GetAllApprovalStepDetailQueryHandler : IRequestHandler<GetAllApprovalStepDetailQuery, ApiResponseDTO<List<ApprovalStepDetailDto>>>
    {
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;
        public GetAllApprovalStepDetailQueryHandler(IApprovalStepDetailQuery approvalStepDetailQuery, IMediator mediator, IMapper mapper, ILookupRepository lookupRepository)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
            _mediator = mediator;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
        }
        public async Task<ApiResponseDTO<List<ApprovalStepDetailDto>>> Handle(GetAllApprovalStepDetailQuery request, CancellationToken cancellationToken)
        {
             var (ApprovalStepDetail, TotalCount) = await _approvalStepDetailQuery.GetAllApprovalStepDetailAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var ApprovalStepDetailDto = _mapper.Map<List<ApprovalStepDetailDto>>(ApprovalStepDetail);

             var menuLookup = await _lookupRepository.GetMenuNamesAsync(ApprovalStepDetailDto.Select(x => x.MenuId), cancellationToken);

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
