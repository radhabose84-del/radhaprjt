using System.Text.Json;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IOutbox;
using MaintenanceManagement.Domain.Entities.Outbox;
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

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public OutboxEventPublisher(
            IOutboxRepository outboxRepository,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            ILogger<OutboxEventPublisher> logger)
        {
            _outboxRepository = outboxRepository;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _logger = logger;
        }

        public async Task ScheduleAsync<TEvent>(
            TEvent @event,
            Guid correlationId,
            CancellationToken cancellationToken = default) where TEvent : class
        {
            var eventType = typeof(TEvent);
            var systemTimeZone = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZone);

            var outboxMessage = new OutboxMessage
            {
                CorrelationId = correlationId,
                EventType = eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name,
                EventData = JsonSerializer.Serialize(@event, eventType, JsonOptions),
                Status = OutboxMessageStatus.Pending,
                CreatedAt = currentTime,
                RetryCount = 0,
                MaxRetries = 5,
                NextRetryAt = null, // Ready for immediate processing
                ModuleName = "MaintenanceManagement",
                CreatedBy = _ipAddressService.GetUserId()
            };

            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);

            _logger.LogDebug(
                "Scheduled outbox event. Type: {EventType}, CorrelationId: {CorrelationId}",
                eventType.Name, correlationId);
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
    }
}
