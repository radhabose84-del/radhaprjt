using BackgroundService.Application.DTO;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface IInAppNotifier
    {
        Task<bool> SendInAppNotificationAsync(List<int> userIds, string message, string title,string Value, NotificationContext context);
    }
}