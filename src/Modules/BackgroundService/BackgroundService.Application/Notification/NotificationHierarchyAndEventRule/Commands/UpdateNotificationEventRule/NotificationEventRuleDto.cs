namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.DTOs
{
    public class NotificationEventRuleDto
    {
        public int Id { get; set; } // NotificationEventRule Id
        public int NotificationChannelId { get; set; }
        public int RecipientTypeId { get; set; }
        public int TemplateId { get; set; }
    }
}
