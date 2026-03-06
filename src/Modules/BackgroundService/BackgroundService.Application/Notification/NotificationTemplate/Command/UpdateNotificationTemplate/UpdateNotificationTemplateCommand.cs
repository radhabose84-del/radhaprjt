using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate
{
    public class UpdateNotificationTemplateCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int NotificationTypeId { get; set; }
        public int NotificationConfigId { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? FooterTemplate { get; set; }
        public string?  LanguageCode { get; set; }
        public byte IsActive { get; set; }
    }
}