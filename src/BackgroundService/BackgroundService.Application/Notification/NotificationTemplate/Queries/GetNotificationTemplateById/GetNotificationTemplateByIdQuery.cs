using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateById
{
    public class GetNotificationTemplateByIdQuery : IRequest<NotificationTemplateDto>
    {
        public int Id { get; set; }
    }
}