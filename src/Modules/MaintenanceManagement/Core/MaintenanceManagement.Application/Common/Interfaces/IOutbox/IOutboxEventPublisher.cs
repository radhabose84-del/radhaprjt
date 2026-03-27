namespace MaintenanceManagement.Application.Common.Interfaces.IOutbox
{
    /// <summary>
    /// High-level abstraction for publishing events via the outbox pattern.
    /// Events are saved to the outbox within the current transaction and
    /// published asynchronously by a background worker.
    /// </summary>
    public interface IOutboxEventPublisher
    {
        /// <summary>
        /// Schedules an event for publication via the outbox pattern.
        /// The event is saved to the database within the current transaction.
        /// A background worker will pick it up and publish to the message broker.
        /// </summary>
        /// <typeparam name="TEvent">Event type (must be serializable)</typeparam>
        /// <param name="event">The event to publish</param>
        /// <param name="correlationId">Unique correlation ID for distributed tracing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task ScheduleAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules multiple events for publication within the same transaction.
        /// </summary>
        Task ScheduleBatchAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules an event WITHOUT saving — participates in caller's transaction.
        /// Use this when outbox must be atomic with other domain operations.
        /// Caller must call SaveChangesAsync() to persist.
        /// </summary>
        /// <param name="processorHint">
        /// Optional routing hint. Pass "maintenance" for direct Hangfire scheduling events
        /// (handled by MaintenanceOutboxProcessorJob). Leave null for MassTransit events
        /// (handled by SqlOutboxProcessorJob).
        /// </param>
        Task ScheduleWithoutSaveAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            string? processorHint = null,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Publishes an event directly to the message broker (RabbitMQ) for immediate delivery.
        /// Call this AFTER CommitAsync() succeeds to bypass the outbox polling delay.
        /// Also marks the outbox message as Published so the outbox processor skips it.
        /// If direct publish fails, the outbox message stays Pending for the processor to retry.
        /// </summary>
        Task PublishDirectAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules multiple events WITHOUT saving — participates in caller's transaction.
        /// </summary>
        Task ScheduleBatchWithoutSaveAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            string? processorHint = null,
            CancellationToken cancellationToken = default) where TEvent : class;
    }
}
