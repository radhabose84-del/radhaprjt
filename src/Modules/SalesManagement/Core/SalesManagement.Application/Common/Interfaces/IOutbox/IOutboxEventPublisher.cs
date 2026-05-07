namespace SalesManagement.Application.Common.Interfaces.IOutbox
{
    public interface IOutboxEventPublisher
    {
        Task ScheduleAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        Task ScheduleWithoutSaveAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        /// <summary>
        /// Commits all pending outbox messages added via <see cref="ScheduleWithoutSaveAsync{TEvent}"/>
        /// in a single atomic database operation.
        /// </summary>
        Task SavePendingAsync(CancellationToken cancellationToken = default);

        Task ScheduleBatchAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;

        Task ScheduleBatchWithoutSaveAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class;
    }
}
