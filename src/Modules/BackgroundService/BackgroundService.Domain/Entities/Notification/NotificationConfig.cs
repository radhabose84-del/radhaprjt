using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationConfig : BaseEntity
    {
        public string? ModuleName { get; set; }
        public int NotificationEventTypeId { get; set; }
        public int UnitId { get; set; }
        public MiscMaster? NotificationEventType { get; set; } 
        public ICollection<NotificationLevelHierarchy> NotificationLevelHierarchies { get; set; }  = new List<NotificationLevelHierarchy>();      
        public ICollection<NotificationTemplate> NotificationTemplates { get; set; }= new List<NotificationTemplate>();
    }
}