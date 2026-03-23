using MassTransit;

namespace Contracts.Events.Sales;

public record ApprovedRejectedSalesCompletedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int ModuleTransactionId { get; init; }
}
