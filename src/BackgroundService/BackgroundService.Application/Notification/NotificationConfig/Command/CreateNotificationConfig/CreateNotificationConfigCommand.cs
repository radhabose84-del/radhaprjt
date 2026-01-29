using BackgroundService.Application.Notification.Common.HttpResponse;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig
{
    public class CreateNotificationConfigCommand : IRequest<int>
    {        
        public string? ModuleName { get; set; }       
        public int  NotificationEventTypeId { get; set; }
    }
}