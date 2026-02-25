using MassTransit;

namespace Contracts.Events.Notifications.Whatsapp
{
    public class SendWhatsappNotificationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
