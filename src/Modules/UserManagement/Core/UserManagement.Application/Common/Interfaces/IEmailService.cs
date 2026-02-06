using Contracts.Events.Notifications;

namespace UserManagement.Application.Common.Interfaces
{
    public interface IEmailService
    { 
        Task<bool> SendEmailAsync(SendEmailCommand command);
    }
}

