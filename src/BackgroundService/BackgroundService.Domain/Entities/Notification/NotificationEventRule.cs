using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationEventRule : BaseEntity
    {
        public int NotificationLevelHierarchyId { get; set; }
        public int NotificationChannelId { get; set; }
        public int RecipientTypeId { get; set; }
        public int TemplateId { get; set; }        
        public MiscMaster? RecipientType { get; set; }
        public MiscMaster? Channel { get; set; }
        public NotificationTemplate? NotificationTemplates { get; set; }
        public NotificationLevelHierarchy? NotificationLevelHierarchy { get; set; }
        public ICollection<NotificationEventLog>? NotificationEventLog { get; set; }
        
    }
}