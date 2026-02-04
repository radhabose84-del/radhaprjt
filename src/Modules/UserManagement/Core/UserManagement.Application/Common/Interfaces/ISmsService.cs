using Contracts.Events.Notifications;

namespace UserManagement.Application.Common.Interfaces
{
    public interface ISmsService
    {        
        Task<bool> SendSmsAsync(SendSmsCommand command);
    }
}