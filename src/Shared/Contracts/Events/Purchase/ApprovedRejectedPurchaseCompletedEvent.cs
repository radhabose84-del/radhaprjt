using MassTransit;
namespace Contracts.Events.Purchase;

public record ApprovedRejectedPurchaseCompletedEvent  : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int ModuleTransactionId { get; init; }
}
