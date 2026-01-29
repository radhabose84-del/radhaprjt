using Contracts.Events.Notifications;

namespace Core.Application.Common.Interfaces
{
    public interface ISmsService
    {        
        Task<bool> SendSmsAsync(SendSmsCommand command);
    }
}