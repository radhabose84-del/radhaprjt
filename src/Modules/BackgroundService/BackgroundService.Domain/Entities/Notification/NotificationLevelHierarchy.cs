using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationLevelHierarchy : BaseEntity
    {
        public int NotificationConfigId { get; set; }
        public int TargetTypeId { get; set; }
        public int TargetId { get; set; }
        public int ApprovalModeId { get; set; }
        public string? Description { get; set; }
        public NotificationConfig? NotificationConfig { get; set; }
        public MiscMaster? TargetType { get; set; }
        public MiscMaster? ApprovalMode { get; set; }           
        public ICollection<NotificationEventRule>? NotificationEventRules { get; set; }   
    }
}