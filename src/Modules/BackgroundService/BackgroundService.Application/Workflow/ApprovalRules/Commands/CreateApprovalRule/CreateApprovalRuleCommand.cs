using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class CreateApprovalRuleCommand : IRequest<int>
    {
        public int ActionId { get; set; }
        public int ApprovalStepDetailId { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public int Priority { get; set; }
        public List<ApprovalRuleConditionDto> ApprovalRuleConditions { get; set; }
    }
}