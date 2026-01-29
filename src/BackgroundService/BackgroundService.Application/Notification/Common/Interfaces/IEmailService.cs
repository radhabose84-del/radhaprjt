using Contracts.Events.Notifications;

namespace BackgroundService.Application.Notification.Common.Interfaces
{
    public interface IEmailService
    { 
        Task<bool> SendEmailAsync(SendEmailCommand command);
    }
}

