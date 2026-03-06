namespace BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById
{
    public class GetNotificationDetailDto
    {
        public int Id { get; set; }
        public string? ModuleName { get; set; }
        public string? EventType { get; set; }
        public string? TargetType { get; set; }
        public string? ChannelName { get; set; }
        public string? ActionStatus { get; set; }
        public string? ReadStatus { get; set; }
        public string? ReadStatusId { get; set; }
        public string? MessageText { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public string? SendTo { get; set; }
        public string? Value { get; set; }
    }
}