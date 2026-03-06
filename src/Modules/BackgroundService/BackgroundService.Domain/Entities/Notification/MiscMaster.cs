using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;

namespace BackgroundService.Domain.Entities.Notification
{
    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public MiscTypeMaster MiscType { get; set; } = new MiscTypeMaster();
        public ICollection<NotificationLevelHierarchy> TargetType { get; set; } = new List<NotificationLevelHierarchy>();
        public ICollection<NotificationLevelHierarchy> ApprovalMode { get; set; } = new List<NotificationLevelHierarchy>();
        public ICollection<NotificationEventRule> Channels { get; set; } = new List<NotificationEventRule>();
        public ICollection<NotificationEventRule> RecipientType { get; set; } = new List<NotificationEventRule>();
        public ICollection<NotificationConfig> NotificationEventType { get; set; } = new List<NotificationConfig>();
        public ICollection<NotificationEventLog> Channel { get; set; } = new List<NotificationEventLog>();
        public ICollection<NotificationEventLog> NotificationStatus { get; set; } = new List<NotificationEventLog>();
        public ICollection<NotificationTemplate> NotificationTemplates { get; set; } = new List<NotificationTemplate>();
        public ICollection<NotificationEventLog> ReadStatus { get; set; } = new List<NotificationEventLog>();
        public ICollection<ApprovalStepDetail> ApprovalStep { get; set; }
        public ICollection<ApprovalStepDetail> ApprovalType { get; set; }
        public ICollection<ApprovalRequest> ApprovalRequestStatus { get; set; }
        public ICollection<ApprovalRequestLine> ApprovalRequestLineStatus { get; set; }
        public ICollection<ApprovalRule> Action { get; set; }
        public ICollection<ApprovalStepDetail> ApprovalTargetType { get; set; }
        public ICollection<ApprovalRuleCondition> Operator { get; set; }
        public ICollection<ApprovalRuleCondition> RightType { get; set; }
        public ICollection<ApprovalDataField> ValueType { get; set; }
        public ICollection<ApprovalDataField> Scope { get; set; }
    }
}