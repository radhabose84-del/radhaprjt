using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class RuleSkipApproverMappingByIdDto
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
    }
}