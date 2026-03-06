using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class ApprovalStepDetailByIdDto
    {
        public int Id { get; set; }
        public int WorkFlowTypeId { get; set; }
        public int StepOrder { get; set; }
        public int TargetTypeId { get; set; }
        public int TargetValueId { get; set; }
        public int ApprovalStepId { get; set; }
        public byte StopOnFirstMatch { get; set; }
        public byte IsEdit { get; set; }
        public byte IsActive { get; set; }
        public List<ApprovalStepUnitMappingByIdDto> ApprovalStepUnitMappings { get; set; }
        public List<ApprovalStepDepartmentMappingByIdDto> ApprovalStepDepartmentMappings { get; set; }
        public List<ApprovalRuleByIdDto> ApprovalRules { get; set; }
    }
}