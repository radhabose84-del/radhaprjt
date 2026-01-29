using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using Contracts.Interfaces.External.IUser;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType
{
    public class GetAllWorkflowTypeQueryHandler : IRequestHandler<GetAllWorkflowTypeQuery, ApiResponseDTO<List<WorkflowTypeDto>>>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMenuGrpcClient _menuGrpcClient;
        public GetAllWorkflowTypeQueryHandler(IWorkflowTypeQuery workflowTypeQuery, IMediator mediator, IMapper mapper, IMenuGrpcClient menuGrpcClient)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _mediator = mediator;
            _mapper = mapper;
            _menuGrpcClient = menuGrpcClient;
        }
        public async Task<ApiResponseDTO<List<WorkflowTypeDto>>> Handle(GetAllWorkflowTypeQuery request, CancellationToken cancellationToken)
        {
            var (WorkflowType, TotalCount) = await _workflowTypeQuery.GetAllWorkflowTypeAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var WorkflowTypeDto = _mapper.Map<List<WorkflowTypeDto>>(WorkflowType);

            var menus = await _menuGrpcClient.GetMenuIdsAsync(WorkflowTypeDto.Select(x => x.MenuId).ToList());

            var menuLookup  = menus.ToDictionary(d => d.Id, d => d.MenuName);

            foreach (var dto in WorkflowTypeDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var MenuName))
                {
                    dto.MenuName = MenuName;
                }
            }


            return new ApiResponseDTO<List<WorkflowTypeDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = WorkflowTypeDto,
                TotalCount = TotalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}