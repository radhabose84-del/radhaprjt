using System;
using MassTransit;
namespace Contracts.Events.Purchase;

public record ApprovedRejectedBudgetCompletedEvent : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; init; }
    public int ModuleTransactionId { get; init; }
}
public class ApprovedRejectedBudgetFailedEvent
{
    public Guid CorrelationId { get; set; }
    public int ModuleTransactionId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
