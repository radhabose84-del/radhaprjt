using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig
{
    public class CreateNotificationConfigCommand : IRequest<int>
    {        
        public string? ModuleName { get; set; }       
        public int  NotificationEventTypeId { get; set; }
    }
}