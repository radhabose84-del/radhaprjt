namespace FinanceManagement.Domain.Entities.Outbox
{
    // Transactional outbox row (SQL). Drained to the MassTransit bus by the shared
    // SqlOutboxProcessorJob in BSOFT.Worker. Distinct from the MongoDB OutboxMessage
    // in FinanceManagement.Infrastructure.Persistence (used by the e-invoice flow).
    public class OutboxMessage
    {
        public long Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string EventData { get; set; } = string.Empty;
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; } = 5;
        public string? LastError { get; set; }
        public DateTimeOffset? NextRetryAt { get; set; }
        public string ModuleName { get; set; } = "FinanceManagement";
        public int? CreatedBy { get; set; }
    }

    public enum OutboxMessageStatus
    {
        Pending = 0,
        Published = 1,
        Failed = 2
    }
}
