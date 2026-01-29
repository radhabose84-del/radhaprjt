using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using Contracts.Interfaces.External.IUser;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete
{
    public class GetWorkflowTypeAutoCompleteQueryHandler : IRequestHandler<GetWorkflowTypeAutoCompleteQuery, List<GetWorkflowTypeAutoCompleteDto>>
    {
         private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IMenuGrpcClient _menuGrpcClient;
        public GetWorkflowTypeAutoCompleteQueryHandler(IWorkflowTypeQuery workflowTypeQuery, IMediator mediator, IMapper mapper, IMenuGrpcClient menuGrpcClient)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _mediator = mediator;
            _mapper = mapper;
            _menuGrpcClient = menuGrpcClient;
        }
        public async Task<List<GetWorkflowTypeAutoCompleteDto>> Handle(GetWorkflowTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
             var Result = await _workflowTypeQuery.GetWorkflowTypeAutoComplete(request.SearchPattern ?? string.Empty);
            var WorkflowType = _mapper.Map<List<GetWorkflowTypeAutoCompleteDto>>(Result);

             var menus = await _menuGrpcClient.GetMenuIdsAsync(WorkflowType.Select(x => x.MenuId).ToList());

            var menuLookup  = menus.ToDictionary(d => d.Id, d => d.MenuName);

            foreach (var dto in WorkflowType)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var MenuName))
                {
                    dto.MenuName = MenuName;
                }
            }
            
            return WorkflowType;
        }
    }
}