using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule
{
    public class UpdateApprovalRuleCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public int ActionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public int Priority { get; set; }
        public byte IsActive { get; set; }
        public List<ApprovalRuleConditionUpdateDto> ApprovalRuleConditions { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
