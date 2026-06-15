using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
{
    public class InsertNotificationHierarchyAndEventRuleCommand : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
        public NotificationHierarchyAndEventRuleDto Dto { get; set; }

        public InsertNotificationHierarchyAndEventRuleCommand(NotificationHierarchyAndEventRuleDto dto)
        {
            Dto = dto;
        }
    }
}
