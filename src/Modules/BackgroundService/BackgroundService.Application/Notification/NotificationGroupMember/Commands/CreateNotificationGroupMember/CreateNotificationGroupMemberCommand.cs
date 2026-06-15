using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember
{
    public class CreateNotificationGroupMemberCommand : IRequest<int>, IRequirePermission
    {
        public int GroupId { get; set; }
        public List<int> UserIds { get; set; }  
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
