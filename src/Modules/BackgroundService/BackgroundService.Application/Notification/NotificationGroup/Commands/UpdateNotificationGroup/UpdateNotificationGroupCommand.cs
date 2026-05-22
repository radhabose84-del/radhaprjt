using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationGroup.Commands.UpdateNotificationGroup
{
    public class UpdateNotificationGroupCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
