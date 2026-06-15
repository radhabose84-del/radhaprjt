using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail
{
    public class UpdateApprovalStepDetailCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public int WorkFlowTypeId { get; set; }
        public int StepOrder { get; set; }
        public int TargetTypeId { get; set; }
        public int TargetValueId { get; set; }
        public int ApprovalStepId { get; set; }
        public byte StopOnFirstMatch { get; set; }
        public byte IsActive { get; set; }
        public byte IsEdit { get; set; }
        public List<ApprovalStepUnitMappingUpdateDto> ApprovalStepUnitMappings { get; set; }
        // public List<RuleSkipApproverMappingDto> RuleSkipApproverMappings { get; set; }
        public List<ApprovalStepDepartmentMappingUpdateDto>? ApprovalStepDepartmentMappings { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
