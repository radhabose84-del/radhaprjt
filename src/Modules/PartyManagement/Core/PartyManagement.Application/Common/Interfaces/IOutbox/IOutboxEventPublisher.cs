namespace PartyManagement.Application.Common.Interfaces.IOutbox
{
    public interface IOutboxEventPublisher
    {
        Task ScheduleAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
        Task ScheduleWithoutSaveAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
        Task ScheduleBatchAsync<TEvent>(IEnumerable<TEvent> events, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
        Task ScheduleBatchWithoutSaveAsync<TEvent>(IEnumerable<TEvent> events, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class;
    }
}
