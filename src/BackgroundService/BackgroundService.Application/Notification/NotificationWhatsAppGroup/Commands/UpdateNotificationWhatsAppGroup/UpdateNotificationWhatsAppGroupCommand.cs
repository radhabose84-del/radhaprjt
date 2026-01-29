using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup
{
    public class UpdateNotificationWhatsAppGroupCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public int IsActive { get; set; }
    }
}
