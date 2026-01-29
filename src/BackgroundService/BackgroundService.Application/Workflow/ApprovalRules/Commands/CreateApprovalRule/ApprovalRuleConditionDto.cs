using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class ApprovalRuleConditionDto
    {
        // public int RuleId { get; set; }
        // public int FieldId { get; set; }
        public int OperatorId { get; set; }
        public int RightTypeId { get; set; }
        public string RightValue { get; set; }
        public string Aggregate { get; set; }
        public ApprovalDatafieldDto Datafield { get; set; }
    }
}