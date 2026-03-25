using System.Text.Json.Serialization;
using MassTransit;

namespace Contracts.Events.Notifications.InApp
{
    public class SendInAppNotificationInternalCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int UnitId { get; set; }
        public int EventTypeId { get; set; }
        public string ModuleName { get; set; } = default!;
        public List<int> UserIds { get; set; } = new();
        public int EventRuleId { get; set; }
        public int ChannelId { get; set; }
        public string Email { get; set; } = default!;
        [JsonPropertyName("ccMail")]
        public string ccMail { get; set; } = default!;
        public string Mobile { get; set; } = default!;
        public string CreatedByName { get; set; } = string.Empty;
        public string param1 { get; set; } = string.Empty;
        public string param2 { get; set; } = string.Empty;
        public DateTimeOffset param3 { get; set; }    
        public string param4 { get; set; } = string.Empty;
        public string param5 { get; set; } = string.Empty;
        public string param6 { get; set; } = string.Empty;
        public string param7 { get; set; } = string.Empty;
        public string param8 { get; set; } = string.Empty;
        public string param9 { get; set; } = string.Empty;
        public string param10 { get; set; } = string.Empty;  
        public int RetryCount { get; set; } = 0; 
    }
}
