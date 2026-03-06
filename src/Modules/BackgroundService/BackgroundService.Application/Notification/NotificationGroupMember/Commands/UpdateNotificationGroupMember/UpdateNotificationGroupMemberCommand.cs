using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Commands.UpdateNotificationGroupMember
{
    public class UpdateNotificationGroupMemberCommand : IRequest<bool>
    {         
        public int GroupId { get; set; }
        public List<int> UserIds { get; set; }
        public byte IsActive { get; set; }
    }
}