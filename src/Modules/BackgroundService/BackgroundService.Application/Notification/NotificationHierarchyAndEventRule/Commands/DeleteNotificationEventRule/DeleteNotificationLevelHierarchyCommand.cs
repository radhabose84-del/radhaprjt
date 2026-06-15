using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule
{
    public class DeleteNotificationLevelHierarchyCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
