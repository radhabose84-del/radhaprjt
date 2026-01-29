using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup
{
    public class CreateNotificationGroupCommand : IRequest<int>
    {
        public string GroupName { get; set; }
    }
}