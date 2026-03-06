using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
    public class NotificationGroupMembers : BaseEntity
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public NotificationGroup Group { get; set; }
        
    }
}