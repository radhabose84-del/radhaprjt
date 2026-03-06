using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailAutoComplete
{
    public class ApprovalStepDetailAutoCompleteDto
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string ApprovalStep { get; set; }
        public string ApproverName { get; set; }
        public int TargetValueId { get; set; }
    }
}