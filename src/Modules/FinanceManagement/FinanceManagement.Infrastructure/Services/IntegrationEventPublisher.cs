using FinanceManagement.Application.Common.Interfaces.IIntegrationEvents;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.Infrastructure.Services
{
    // US-GL02-16 (AC3) — publish a cross-module event within 1s. Happy path = direct bus publish
    // (sub-second). If the broker is momentarily unavailable, fall back to the transactional outbox
    // (durable, dispatched by the worker) so delivery is still guaranteed — no duplicate on success.
    public class IntegrationEventPublisher : IIntegrationEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly ILogger<IntegrationEventPublisher> _logger;

        public IntegrationEventPublisher(
            IPublishEndpoint publishEndpoint,
            IOutboxEventPublisher outboxEventPublisher,
            ILogger<IntegrationEventPublisher> logger)
        {
            _publishEndpoint = publishEndpoint;
            _outboxEventPublisher = outboxEventPublisher;
            _logger = logger;
        }

        public async Task PublishWithinSlaAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken ct = default)
            where TEvent : class
        {
            try
            {
                await _publishEndpoint.Publish(@event, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Direct publish of {EventType} failed; falling back to the transactional outbox.",
                    typeof(TEvent).Name);
                // ScheduleAsync persists immediately (own transaction); the worker dispatches it.
                await _outboxEventPublisher.ScheduleAsync(@event, correlationId, ct);
            }
        }
    }
}
