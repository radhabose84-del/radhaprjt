using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationGroupMembers
{
    public interface INotificationGroupMemberCommand
    {
        // Task<int> CreateAsync(NotificationGroupMembers notificationGroupMembers);     
        Task<int> CreateMultipleAsync(IEnumerable<NotificationGroupMembers> members);
        Task<bool> UpdateMultipleAsync(int groupId, List<int> userIds, byte isActive);
        //Task<bool> DeleteAsync(int id,NotificationGroupMembers notificationGroupMembers); 
    }
}