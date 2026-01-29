using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigById
{
    public class GetNotificationConfigByIdQuery : IRequest<NotificationConfigDto>
    {
        public int Id { get; set; }
    }
}