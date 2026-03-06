using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class ApprovalRuleByIdDto
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public int Priority { get; set; }
        public List<ApprovalRuleConditionByIdDto> ApprovalRuleConditions { get; set; }
    }
}