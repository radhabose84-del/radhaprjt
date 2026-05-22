using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class CreateApprovalRuleCommand : IRequest<int>, IRequirePermission
    {
        public int ActionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public int Priority { get; set; }
        public List<ApprovalRuleConditionDto> ApprovalRuleConditions { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
