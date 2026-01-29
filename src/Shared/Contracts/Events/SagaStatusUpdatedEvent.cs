using System;
using MassTransit;

namespace Contracts.Events
{
    public class SagaStatusUpdatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int WorkOrderId { get; set; }
        public string Status { get; set; } = "Completed"; // "Completed" or "Failed"
        public string? FailureReason { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}