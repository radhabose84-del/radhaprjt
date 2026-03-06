using Contracts.Events.Notifications.Sms;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface ISmsSender
    {
        Task<bool> SendSmsAsyncOld(List<string> mobileNumbers, string message);
        Task<bool> SendSmsAsync(SendSmsNotificationCommand command);
    }
}