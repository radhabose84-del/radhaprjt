using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationTemplate : BaseEntity
    {
        public int NotificationTypeId { get; set; }
        public int NotificationConfigId { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? LanguageCode { get; set; }
        public string? FooterTemplate { get; set; }       
        public bool IsTable { get; set; } 
        public MiscMaster? NotificationType { get; set; }
        public NotificationConfig NotificationConfig { get; set; }=null!;
        public ICollection<NotificationEventRule> NotificationEventRules { get; set; }=new List<NotificationEventRule>();
    }
}