using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig
{
    public class DeleteNotificationConfigCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
