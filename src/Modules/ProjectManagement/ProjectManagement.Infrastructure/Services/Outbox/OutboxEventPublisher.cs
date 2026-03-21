using System.Text.Json;
using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Application.Common.Interfaces.IOutbox;
using ProjectManagement.Domain.Entities.Outbox;
using Microsoft.Extensions.Logging;

namespace ProjectManagement.Infrastructure.Services.Outbox
{
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

        public async Task ScheduleAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            var outboxMessage = CreateOutboxMessage(@event, correlationId);
            await _outboxRepository.AddAsync(outboxMessage, cancellationToken);
            _logger.LogDebug("Scheduled outbox event. Type: {EventType}, CorrelationId: {CorrelationId}", typeof(TEvent).Name, correlationId);
        }

        public async Task ScheduleWithoutSaveAsync<TEvent>(TEvent @event, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            var outboxMessage = CreateOutboxMessage(@event, correlationId);
            await _outboxRepository.AddWithoutSaveAsync(outboxMessage, cancellationToken);
        }

        public async Task ScheduleBatchAsync<TEvent>(IEnumerable<TEvent> events, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            foreach (var @event in events)
                await ScheduleAsync(@event, correlationId, cancellationToken);
        }

        public async Task ScheduleBatchWithoutSaveAsync<TEvent>(IEnumerable<TEvent> events, Guid correlationId, CancellationToken cancellationToken = default) where TEvent : class
        {
            foreach (var @event in events)
                await ScheduleWithoutSaveAsync(@event, correlationId, cancellationToken);
        }

        private OutboxMessage CreateOutboxMessage<TEvent>(TEvent @event, Guid correlationId) where TEvent : class
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
                ModuleName = "ProjectManagement",
                CreatedBy = _ipAddressService.GetUserId()
            };
        }
    }
}
