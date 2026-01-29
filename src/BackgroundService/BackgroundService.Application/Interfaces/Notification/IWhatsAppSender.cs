using System.Threading.Tasks;
using Contracts.Events.Notifications.Whatsapp;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface IWhatsAppSender
    {
        Task<bool> SendWhatsAppAsync(SendWhatsappNotificationCommand command);
    }

    public class SendWhatsappNotificationCommand
    {
        public string? MobileNumber { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        
    }
}
