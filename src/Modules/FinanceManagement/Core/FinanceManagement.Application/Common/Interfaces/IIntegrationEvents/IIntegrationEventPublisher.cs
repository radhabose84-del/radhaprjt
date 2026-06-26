namespace FinanceManagement.Application.Common.Interfaces.IIntegrationEvents
{
    // US-GL02-16 (AC3) — publish a cross-module integration event within the 1-second SLA.
    // Implementation publishes to the bus directly on the happy path and falls back to the
    // transactional outbox if the broker is momentarily unavailable (durable, no duplicate on success).
    public interface IIntegrationEventPublisher
    {
        Task PublishWithinSlaAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken ct = default)
            where TEvent : class;
    }
}
