using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationGroup : BaseEntity
    {
        public string? GroupName { get; set; }
        public int UnitId { get; set; }
        public ICollection<NotificationGroupMembers> NotificationGroupMembers { get; set; }        
    }
}