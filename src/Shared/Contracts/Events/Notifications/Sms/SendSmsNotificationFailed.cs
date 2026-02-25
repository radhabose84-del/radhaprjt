using MassTransit;

namespace Contracts.Events.Notifications.Sms
{
    public class SendSmsNotificationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
