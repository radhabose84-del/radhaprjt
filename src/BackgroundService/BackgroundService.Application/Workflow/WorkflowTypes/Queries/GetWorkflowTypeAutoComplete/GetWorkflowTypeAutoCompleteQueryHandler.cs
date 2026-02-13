using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete
{
    public class GetWorkflowTypeAutoCompleteQueryHandler : IRequestHandler<GetWorkflowTypeAutoCompleteQuery, List<GetWorkflowTypeAutoCompleteDto>>
    {
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        private readonly IMapper _mapper;
        private readonly ILookupRepository _lookupRepository;

        public GetWorkflowTypeAutoCompleteQueryHandler(
            IWorkflowTypeQuery workflowTypeQuery,
            IMapper mapper,
            ILookupRepository lookupRepository)
        {
            _workflowTypeQuery = workflowTypeQuery;
            _mapper = mapper;
            _lookupRepository = lookupRepository;
        }

        public async Task<List<GetWorkflowTypeAutoCompleteDto>> Handle(GetWorkflowTypeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var Result = await _workflowTypeQuery.GetWorkflowTypeAutoComplete(request.SearchPattern ?? string.Empty);
            var WorkflowType = _mapper.Map<List<GetWorkflowTypeAutoCompleteDto>>(Result);

            var menuLookup = await _lookupRepository.GetMenuNamesAsync(
                WorkflowType.Select(x => x.MenuId),
                cancellationToken);

            foreach (var dto in WorkflowType)
            {
                if (menuLookup.TryGetValue(dto.MenuId, out var menuName))
                {
                    dto.MenuName = menuName;
                }
            }

            return WorkflowType;
        }
    }
}
