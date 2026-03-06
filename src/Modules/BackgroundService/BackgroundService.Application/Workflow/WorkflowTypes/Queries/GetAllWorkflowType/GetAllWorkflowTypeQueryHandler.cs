using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetAllWorkflowType
{
    public class GetAllWorkflowTypeQueryHandler : IRequestHandler<GetAllWorkflowTypeQuery, ApiResponseDTO<List<WorkflowTypeDto>>>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;

        public GetAllWorkflowTypeQueryHandler(
            IWorkflowTypeQuery workflowTypeQuery,
            IMapper mapper,
            ILookupRepository lookupRepository)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
        }

        public async Task<ApiResponseDTO<List<WorkflowTypeDto>>> Handle(GetAllWorkflowTypeQuery request, CancellationToken cancellationToken)
        {
            var (WorkflowType, TotalCount) = await _workflowTypeQuery.GetAllWorkflowTypeAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var WorkflowTypeDto = _mapper.Map<List<WorkflowTypeDto>>(WorkflowType);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(
                WorkflowTypeDto.Select(x => x.MenuId),
                cancellationToken);

            foreach (var dto in WorkflowTypeDto)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var menuName))
                {
                    dto.MenuName = menuName;
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
