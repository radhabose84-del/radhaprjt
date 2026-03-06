namespace BackgroundService.Infrastructure.Persistence
{
    /// <summary>
    /// Tracks processed messages to prevent duplicate processing on MassTransit redelivery.
    /// Keyed by (ConsumerName, MessageId) — unique index enforced at DB level.
    /// </summary>
    public class InboxMessage
    {
        public long Id { get; set; }

        /// <summary>The fully-qualified consumer class name (e.g. "SendEmailNotificationConsumer").</summary>
        public string ConsumerName { get; set; } = string.Empty;

        /// <summary>MassTransit MessageId — preserved across retries, unique per logical message.</summary>
        public Guid MessageId { get; set; }

        /// <summary>Business-level CorrelationId from the message payload (for tracing).</summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>UTC timestamp when the message was successfully processed.</summary>
        public DateTimeOffset ProcessedAt { get; set; }
    }
}
