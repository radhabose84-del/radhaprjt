    using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;
    using MediatR;

    namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
    {
        public class UpdateNotificationHierarchyAndEventRuleCommand : IRequest<bool>
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
        }
    }
