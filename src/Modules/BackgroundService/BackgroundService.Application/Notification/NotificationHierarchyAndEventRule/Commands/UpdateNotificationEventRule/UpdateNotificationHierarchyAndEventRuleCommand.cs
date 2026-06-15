    using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
    using MediatR;
using Contracts.Common;

    namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
    {
        public class UpdateNotificationHierarchyAndEventRuleCommand : IRequest<bool>, IRequirePermission
        {
            // NotificationLevelHierarchy
            public int NotificationLevelHierarchyId { get; set; }
            public int NotificationConfigId { get; set; }
            public int TargetTypeId { get; set; }
            public int TargetId { get; set; }
            public int ApprovalModeId { get; set; }
            public string? Description { get; set; }
            public byte IsActive { get; set; }

        public List<NotificationEventRuleDto> NotificationEventRules { get; set; } = new();
            public PermissionType RequiredPermission => PermissionType.CanUpdate;
        }
    }
