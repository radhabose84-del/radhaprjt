using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Common;

namespace BackgroundService.Core.Domain.Entities.Notifications
{
    public class NotificationWhatsAppGroup : BaseEntity
    {
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
    }
}