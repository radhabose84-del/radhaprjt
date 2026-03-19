namespace MaintenanceManagement.Domain.Entities.Outbox
{
    /// <summary>
    /// SQL-based Outbox Message entity for transactional outbox pattern.
    /// Stored in the same database as domain entities to ensure atomicity.
    /// </summary>
    public class OutboxMessage
    {
        public long Id { get; set; }

        /// <summary>
        /// Unique identifier for tracing across distributed systems
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// Assembly-qualified type name for deserialization
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// JSON-serialized event payload
        /// </summary>
        public string EventData { get; set; } = string.Empty;

        /// <summary>
        /// Processing status: 0=Pending, 1=Published, 2=Failed
        /// </summary>
        public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;

        /// <summary>
        /// When the message was created (within the transaction)
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the message was successfully published to the message broker
        /// </summary>
        public DateTimeOffset? PublishedAt { get; set; }

        /// <summary>
        /// Number of publish attempts
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Maximum retry attempts before marking as failed
        /// </summary>
        public int MaxRetries { get; set; } = 5;

        /// <summary>
        /// Last error message if publish failed
        /// </summary>
        public string? LastError { get; set; }

        /// <summary>
        /// Next scheduled retry time (for exponential backoff)
        /// </summary>
        public DateTimeOffset? NextRetryAt { get; set; }

        /// <summary>
        /// Module that created this message (for filtering/debugging)
        /// </summary>
        public string ModuleName { get; set; } = "MaintenanceManagement";

        /// <summary>
        /// User who triggered the action
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Hints which processor should handle this message.
        /// NULL = SqlOutboxProcessorJob (general, via MassTransit).
        /// "maintenance" = MaintenanceOutboxProcessorJob (direct Hangfire scheduling).
        /// </summary>
        public string? ProcessorHint { get; set; }
    }

    public enum OutboxMessageStatus
    {
        Pending = 0,
        Published = 1,
        Failed = 2
    }
}
