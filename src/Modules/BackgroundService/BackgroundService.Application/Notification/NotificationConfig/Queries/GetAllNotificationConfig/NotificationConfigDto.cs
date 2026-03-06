namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig
{
    public class NotificationConfigDto
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public int NotificationEventTypeId { get; set; }
        public string? Code { get; set; }       
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