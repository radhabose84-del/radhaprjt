using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Domain.Entities.Workflow
{
    public class ApprovalRuleCondition : BaseEntity
    {
        public int RuleId { get; set; }
        public int FieldId { get; set; }
        public int OperatorId { get; set; }
        public MiscMaster Operator { get; set; }
        public int RightTypeId { get; set; }
        public MiscMaster RightType { get; set; }
        public string RightValue { get; set; }
        public string? Aggregate { get; set; }
        public ApprovalRule Rule { get; set; } = null!;
        public ApprovalDataField Field { get; set; } = null!;
    }
}