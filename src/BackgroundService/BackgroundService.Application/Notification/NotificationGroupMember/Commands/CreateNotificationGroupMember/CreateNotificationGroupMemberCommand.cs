using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Commands.CreateNotificationGroupMember
{
    public class CreateNotificationGroupMemberCommand : IRequest<int>
    {
        public int GroupId { get; set; }
        public List<int> UserIds { get; set; }  
    }
}