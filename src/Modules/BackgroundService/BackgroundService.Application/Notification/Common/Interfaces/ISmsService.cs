using Contracts.Events.Notifications;

namespace BackgroundService.Application.Notification.Common.Interfaces
{
    public interface ISmsService
    {        
        Task<bool> SendSmsAsync(SendSmsCommand command);
    }
}