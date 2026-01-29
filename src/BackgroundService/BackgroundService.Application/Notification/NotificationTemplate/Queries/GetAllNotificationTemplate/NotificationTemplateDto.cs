namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate
{
    public class NotificationTemplateDto
    {
        public int Id { get; set; }
        public int NotificationTypeId { get; set; }
        public int NotificationConfigId { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? FooterTemplate { get; set; }
        public string?  LanguageCode { get; set; }
        public string?  ModuleName { get; set; }
        public string?  ChannelName { get; set; }        
        public int IsActive { get; set; }
        public int IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

    }
}