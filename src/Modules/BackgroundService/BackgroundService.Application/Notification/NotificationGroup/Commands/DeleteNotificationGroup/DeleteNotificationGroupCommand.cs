using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.DeleteNotificationGroup
{
    public class DeleteNotificationGroupCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}