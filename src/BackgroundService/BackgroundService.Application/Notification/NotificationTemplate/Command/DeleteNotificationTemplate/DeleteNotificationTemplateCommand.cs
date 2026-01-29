using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate
{
    public class DeleteNotificationTemplateCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}