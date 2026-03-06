using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig
{
    public class DeleteNotificationConfigCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}