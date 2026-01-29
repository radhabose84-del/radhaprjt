using System;
using System.Collections.Generic;
using MassTransit;

namespace Contracts.Events.Notifications
{
    public class NotificationCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string CreatedByName { get; set; }
        public int UnitId { get; set; }
        public int EventTypeId { get; set; }
        public string ModuleName { get; set; }
        public string Email { get; set; }
        public string ccMail { get; set; }
        public string Mobile { get; set; }
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
        public string? TablePresetKey { get; set; }
        public string? TableRowsJson { get; set; }
        public List<NotificationAttachment> Attachments { get; set; } = new();
                
        public sealed class NotificationAttachment
        {
            public string FileName { get; set; } = "";
            public string ContentType { get; set; } = "application/pdf";
            public string BlobUrl { get; set; } = "";        
            public bool IsPrivate { get; set; } = true;      
        }
    }
}
