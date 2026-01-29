using MediatR;

namespace BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup
{
    public class DeleteNotificationWhatsAppGroupCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
