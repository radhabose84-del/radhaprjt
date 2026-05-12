using System.Text.Json.Serialization;
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
        [JsonPropertyName("ccMail")]
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

        /// <summary>
        /// When set (and non-empty), the InApp consumer ignores the SP-resolved user list
        /// and sends only to these specific user ids. Other channels (Email/SMS/WhatsApp)
        /// are unaffected. Used by handlers that resolve recipients in code (e.g.,
        /// Sales Order → specific Marketing Officer for the order's agent).
        /// </summary>
        public List<int>? OverrideTargetUserIds { get; set; }

        /// <summary>
        /// The ID of the originating module transaction (e.g., SalesQuotation Id, PurchaseOrder Id).
        /// Stored in NotificationEventLog for traceability.
        /// </summary>
        public int ModuleTransactionId { get; set; }

        /// <summary>
        /// The type name of the originating module transaction (e.g., "Sales Quotation", "Sales Order").
        /// Stored in NotificationEventLog for traceability.
        /// </summary>
        public string? ModuleTypeName { get; set; }

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
