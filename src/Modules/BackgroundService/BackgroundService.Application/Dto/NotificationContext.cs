namespace BackgroundService.Application.DTO
{
    public class NotificationContext
    {
        public int EventRuleId { get; set; }
        public int EventTypeId { get; set; }
        public int ChannelId { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int CreatedById { get; set; }
        public string CreatedIp { get; set; } = string.Empty;
        public int? UnitId { get; set; } 
    }
}
