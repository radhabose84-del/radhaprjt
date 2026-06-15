using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate
{
    public class CreateNotificationTemplateCommand : IRequest<int>, IRequirePermission
    {
        public int NotificationTypeId { get; set; }
        public int NotificationConfigId { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? HeaderTemplate { get; set; }        
        public string? BodyTemplate { get; set; }
        public string? FooterTemplate { get; set; }
        public string?  LanguageCode { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
