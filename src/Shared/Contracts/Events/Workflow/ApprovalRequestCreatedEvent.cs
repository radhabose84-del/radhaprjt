using MassTransit;

namespace Contracts.Events.Workflow
{
    public class ApprovalRequestCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}