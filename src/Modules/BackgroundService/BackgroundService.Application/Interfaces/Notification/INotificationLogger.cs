using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface INotificationLogger
    {
        Task<int> LogAsync(NotificationEventLog log); 
    }
}