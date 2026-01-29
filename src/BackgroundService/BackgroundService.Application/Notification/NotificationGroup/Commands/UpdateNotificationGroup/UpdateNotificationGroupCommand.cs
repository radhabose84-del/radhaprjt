using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup
{
    public class UpdateNotificationGroupCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public byte IsActive { get; set; }
    }
}