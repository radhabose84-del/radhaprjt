using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Queries.GetWorkflowTypeAutoComplete
{
    public class GetWorkflowTypeAutoCompleteDto
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public byte IsMultiselect { get; set; }
    }
}