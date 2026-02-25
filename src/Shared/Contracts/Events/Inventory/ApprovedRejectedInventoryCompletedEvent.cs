using MassTransit;

namespace Contracts.Events.Inventory
{
    public class ApprovedRejectedInventoryCompletedEvent : CorrelatedBy<Guid>
    {
            public Guid CorrelationId { get; init; }
            public int ModuleTransactionId { get; init; }
    }
}