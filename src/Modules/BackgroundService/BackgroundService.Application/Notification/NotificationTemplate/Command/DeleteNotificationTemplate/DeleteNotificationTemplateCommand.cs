using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate
{
    public class DeleteNotificationTemplateCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
