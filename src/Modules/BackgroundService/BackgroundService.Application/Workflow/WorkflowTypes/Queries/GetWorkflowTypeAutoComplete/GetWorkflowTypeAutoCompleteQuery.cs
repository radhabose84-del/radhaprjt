using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete
{
    public class GetWorkflowTypeAutoCompleteQuery : IRequest<List<GetWorkflowTypeAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; } 
    }
}