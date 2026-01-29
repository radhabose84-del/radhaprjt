using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
{
    public class NotificationHierarchyAndEventRuleDto
    {
        // NotificationLevelHierarchy
        public int Id { get; set; }
        public int NotificationConfigId { get; set; }
        public int TargetTypeId { get; set; }
        public int TargetId { get; set; }
        public int ApprovalModeId { get; set; }
        public string? Description { get; set; }
        public byte IsActive { get; set; }

        // NotificationEventRule/* 
        /*public int? NotificationEventRuleId { get; set; }
         public int NotificationChannelId { get; set; }
         public int RecipientTypeId { get; set; }
         public int TemplateId { get; set; } */

        public List<NotificationEventRuleDto> NotificationEventRules { get; set; } = new();
    }
}
