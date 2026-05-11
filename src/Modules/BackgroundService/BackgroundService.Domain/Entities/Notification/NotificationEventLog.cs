using BackgroundService.Domain.Common;

namespace BackgroundService.Domain.Entities.Notification
{
   public class NotificationEventLog : BaseEntity
    {
        public int? NotificationLevelRuleId { get; set; }   // <- nullable FK
        public int UnitId { get; set; }
        public int ChannelId { get; set; }
        public int NotificationStatusId { get; set; }
        public string? MessageText { get; set; }
        public string? ActionStatus { get; set; }
        public int ReadStatusId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string? SendTo { get; set; }
        public string? Value { get; set; }
        public int ModuleTransactionId { get; set; }
        public string? ModuleTypeName { get; set; }
        public NotificationEventRule? NotificationEventRules { get; set; }
        public MiscMaster? Channel { get; set; }
        public MiscMaster? NotificationStatus { get; set; }
        public MiscMaster? ReadStatus { get; set; }
    }
}
