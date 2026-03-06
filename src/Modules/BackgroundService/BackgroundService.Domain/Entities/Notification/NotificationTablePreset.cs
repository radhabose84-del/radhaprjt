using System;

namespace BackgroundService.Domain.Entities.Notification
{
    public sealed class NotificationTablePreset
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string PresetKey { get; set; } = string.Empty;
        public string ColumnsJson { get; set; } = string.Empty;
        public int? Version { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTimeOffset CreatedAt { get; set; }
        public byte[]? RowVersion { get; set; }
        public NotificationTemplate? Template { get; set; } 
    }
}
