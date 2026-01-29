using System;
using System.Collections.Generic;
using MassTransit;

namespace Contracts.Events.Notifications
{
    public record SendNotificationInternalCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int UnitId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public int EventTypeId { get; set; }
        public string CreatedByName { get; set; }
        public int EventRuleId { get; set; }
        public string Email { get; set; }
        public string ccMail { get; set; }
        public string Mobile { get; set; }
        public int ChannelId { get; set; }
        public string param1 { get; set; }
        public string param2 { get; set; }
        public DateTimeOffset param3 { get; set; }
        public string param4 { get; set; }
        public string param5 { get; set; }
        public string param6 { get; set; }
        public string param7 { get; set; }
        public string param8 { get; set; }
        public string param9 { get; set; }
        public string param10 { get; set; }
        public List<NotificationCreatedEvent.NotificationAttachment> Attachments { get; set; } = new();
    }
    
    public record NotificationAttachment
    {
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = "application/octet-stream";
        public string BlobUrl { get; init; } = string.Empty;
        public bool IsPrivate { get; init; } = false;
    }

}
