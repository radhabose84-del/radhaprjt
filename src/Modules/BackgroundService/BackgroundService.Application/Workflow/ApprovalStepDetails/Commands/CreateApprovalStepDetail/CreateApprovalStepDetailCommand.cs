using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail
{
    public class CreateApprovalStepDetailCommand : IRequest<int>, IRequirePermission
    {
        public int WorkFlowTypeId { get; set; }
        public int StepOrder { get; set; }
        public int TargetTypeId { get; set; }
        public int TargetValueId { get; set; }
        public int ApprovalStepId { get; set; }
        public byte StopOnFirstMatch { get; set; }
        public byte IsEdit { get; set; }

        public List<ApprovalStepUnitMappingDto> ApprovalStepUnitMappings { get; set; }
        // public List<ApprovalRuleDto> ApprovalRules { get; set; }
        public List<ApprovalStepDepartmentMappingDto>? ApprovalStepDepartmentMappings { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
