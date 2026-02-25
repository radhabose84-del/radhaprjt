using MassTransit;

namespace Contracts.Events.Notifications
{
    public class NotificationCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string CreatedByName { get; set; } = default!;
        public int UnitId { get; set; }
        public int EventTypeId { get; set; }
        public string ModuleName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string ccMail { get; set; } = default!;
        public string Mobile { get; set; } = default!;
        public string param1 { get; set; } = default!;
        public string param2 { get; set; } = default!;
        public DateTimeOffset param3 { get; set; }
        public string param4 { get; set; } = default!;
        public string param5 { get; set; } = default!;
        public string param6 { get; set; } = default!;
        public string param7 { get; set; } = default!;
        public string param8 { get; set; } = default!;
        public string param9 { get; set; } = default!;
        public string param10 { get; set; } = default!;
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
