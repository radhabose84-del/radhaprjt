using MassTransit;

namespace Contracts.Events.Notifications.InApp
{
    public class SendInAppNotificationFailed : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
