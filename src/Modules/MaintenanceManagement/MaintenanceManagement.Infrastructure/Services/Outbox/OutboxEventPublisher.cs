using System.Text.Json;
using Contracts.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Domain.Entities.Outbox;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Infrastructure.Services.Outbox
{
    /// <summary>
    /// Saves events to the outbox table within the current database transaction.
    /// Events are published asynchronously by OutboxPublisherBackgroundService.
    /// </summary>
    public class OutboxEventPublisher : IOutboxEventPublisher
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger<OutboxEventPublisher> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public OutboxEventPublisher(
            IOutboxRepository outboxRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            ILogger<OutboxEventPublisher> logger,
            IPublishEndpoint publishEndpoint)
        {
            _outboxRepository = outboxRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task ScheduleAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            var outboxMessage = CreateOutboxMessage(@event, correlationId);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            _logger.LogDebug(
                "Scheduled outbox event (with save). Type: {EventType}, CorrelationId: {CorrelationId}",
                typeof(TEvent).Name, correlationId);
        }

        public async Task ScheduleWithoutSaveAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            string? processorHint = null,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            var outboxMessage = CreateOutboxMessage(@event, correlationId, processorHint);
            await _outboxRepository.AddWithoutSaveAsync(outboxMessage, cancellationToken);

            _logger.LogDebug(
                "Scheduled outbox event (without save - participates in caller's transaction). Type: {EventType}, CorrelationId: {CorrelationId}, ProcessorHint: {ProcessorHint}",
                typeof(TEvent).Name, correlationId, processorHint ?? "null");
        }

        public async Task ScheduleBatchAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            foreach (var @event in events)
            {
                await ScheduleAsync(@event, correlationId, cancellationToken);
            }
        }

        public async Task ScheduleBatchWithoutSaveAsync<TEvent>(
            IEnumerable<TEvent> events,
            Guid correlationId,
            string? processorHint = null,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            foreach (var @event in events)
            {
                await ScheduleWithoutSaveAsync(@event, correlationId, processorHint, cancellationToken);
            }
        }

        public async Task PublishDirectAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            try
            {
                await _publishEndpoint.Publish(@event, cancellationToken);

                // Mark outbox message as Published so sql-outbox-processor skips it
                var outboxMessage = await _outboxRepository.GetByCorrelationIdAsync(correlationId, cancellationToken);
                if (outboxMessage != null)
                {
                    await _outboxRepository.MarkAsPublishedAsync(outboxMessage.Id, cancellationToken);
                }

                _logger.LogInformation(
                    "Direct-published event to RabbitMQ and marked outbox as Published. CorrelationId: {CorrelationId}",
                    correlationId);
            }
            catch (Exception ex)
            {
                // Don't throw — the outbox processor will pick it up as a fallback
                _logger.LogWarning(ex,
                    "Direct publish failed (outbox will retry). CorrelationId: {CorrelationId}",
                    correlationId);
            }
        }

        private OutboxMessage CreateOutboxMessage<TEvent>(TEvent @event, Guid correlationId, string? processorHint = null) where TEvent : class
        {
            var eventType = typeof(TEvent);
            var systemTimeZone = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZone);

            return new OutboxMessage
            {
                CorrelationId = correlationId,
                EventType = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
                EventData = JsonSerializer.Serialize(@event, eventType, JsonOptions),
                Status = OutboxMessageStatus.Pending,
                CreatedAt = currentTime,
                RetryCount = 0,
                MaxRetries = 5,
                NextRetryAt = null,
                ModuleName = "MaintenanceManagement",
                CreatedBy = _ipAddressService.GetUserId(),
                ProcessorHint = processorHint
            };
        }
    }
}
