using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class ApprovalRuleConditionByIdDto
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public int FieldId { get; set; }
        public int OperatorId { get; set; }
        public int RightTypeId { get; set; }
        public int RightValueId { get; set; }
        public string Aggregate { get; set; }
        public ApprovalDatafieldByIdDto Datafield { get; set; }
    }
}