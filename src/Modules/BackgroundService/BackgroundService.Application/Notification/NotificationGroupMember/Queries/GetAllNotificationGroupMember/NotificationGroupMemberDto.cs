using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember
{
    public class NotificationGroupMemberDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }        
        public string? GroupName { get; set; }
        public string? UserName { get; set; }
        public int IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int ModifiedBy { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}