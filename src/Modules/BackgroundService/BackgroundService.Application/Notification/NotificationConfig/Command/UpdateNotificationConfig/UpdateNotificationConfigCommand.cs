using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig
{
    public class UpdateNotificationConfigCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public int NotificationEventTypeId { get; set; }
        public byte IsActive { get; set; }
    }
}