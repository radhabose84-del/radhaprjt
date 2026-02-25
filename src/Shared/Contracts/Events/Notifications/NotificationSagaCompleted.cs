using MassTransit;

namespace Contracts.Events.Notifications
{
    public class NotificationSagaCompleted : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
