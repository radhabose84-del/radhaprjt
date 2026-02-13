namespace PurchaseManagement.Application.Common.Interfaces.IOutbox
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
        /// Saves to database immediately (separate transaction).
        /// </summary>
        Task ScheduleAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules an event without saving to database.
        /// Participates in caller's transaction - caller must save changes.
        /// Use this when you need atomic transaction with other operations.
        /// </summary>
        Task ScheduleWithoutSaveAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules multiple events for publication (saves immediately).
        /// </summary>
        Task ScheduleBatchAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Schedules multiple events without saving.
        /// Participates in caller's transaction.
        /// </summary>
        Task ScheduleBatchWithoutSaveAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;
    }
}
