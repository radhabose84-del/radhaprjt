using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig
{
    public class UpdateNotificationConfigCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public int NotificationEventTypeId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
