using Contracts.Events.Notifications;

namespace Core.Application.Common.Interfaces
{
    public interface IEmailService
    { 
        Task<bool> SendEmailAsync(SendEmailCommand command);
    }
}

