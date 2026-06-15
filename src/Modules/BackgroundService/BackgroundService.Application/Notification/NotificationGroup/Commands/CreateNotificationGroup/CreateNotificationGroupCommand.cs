using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.CreateNotificationGroup
{
    public class CreateNotificationGroupCommand : IRequest<int>, IRequirePermission
    {
        public string GroupName { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
