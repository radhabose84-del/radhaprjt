using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup
{
    public interface INotificationGroupCommand
    {
        Task<int> CreateAsync(Domain.Entities.Notification.NotificationGroup notificationGroup);     
        Task<bool> UpdateAsync(Domain.Entities.Notification.NotificationGroup notificationGroup);
        Task<bool> DeleteAsync(int id,Domain.Entities.Notification.NotificationGroup notificationGroup); 
    }
}