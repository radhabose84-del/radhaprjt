using BackgroundService.Application.Dto;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Queries.GetNotificationWhatsAppGroupById
{
    public class GetNotificationWhatsAppGroupByIdQuery : IRequest<NotificationWhatsAppGroupDto?>
    {
        public int Id { get; set; }
    }
}
