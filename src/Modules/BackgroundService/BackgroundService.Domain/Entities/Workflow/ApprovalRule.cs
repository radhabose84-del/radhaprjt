using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalRule : BaseEntity
    {

        public int ApprovalStepDetailId { get; set; }
        public int Priority { get; set; }
        public int ActionId { get; set; }
        public MiscMaster Action { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly EffectiveTo { get; set; }
        public ICollection<ApprovalRequest> ApprovalRequest { get; set; }
        public ApprovalStepDetail ApprovalStepDetail { get; set; }
        public ICollection<ApprovalRuleCondition> Conditions { get; set; }
        public ICollection<RuleTargetOverride> RuleTargetOverride { get; set; }
    }
}