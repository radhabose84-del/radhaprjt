using System.Reflection;
using System.Text.Json;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Domain.Entities.Outbox;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PurchaseManagement.Infrastructure.Services.Outbox
{
    /// <summary>
    /// Background service that polls the outbox table and publishes pending messages
    /// to RabbitMQ via MassTransit. Ensures reliable delivery with automatic retry.
    /// </summary>
    public class OutboxPublisherBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherBackgroundService> _logger;
        private readonly OutboxOptions _options;

        public OutboxPublisherBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxPublisherBackgroundService> logger,
            IOptions<OutboxOptions> options)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "PurchaseManagement Outbox Publisher Background Service started. Poll interval: {Interval}s, Batch size: {BatchSize}",
                _options.PollIntervalSeconds, _options.BatchSize);
           

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxMessagesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error in outbox publisher loop. Will retry after interval.");
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("PurchaseManagement Outbox Publisher Background Service stopped.");
        }

        private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var messages = await outboxRepository.GetPendingMessagesAsync(
                _options.BatchSize, stoppingToken);

            if (messages.Count == 0)
            {
                return;
            }

            _logger.LogDebug("Processing {Count} pending outbox messages", messages.Count);

            foreach (var message in messages)
            {
                if (stoppingToken.IsCancellationRequested) break;

                await ProcessSingleMessageAsync(
                    message, outboxRepository, publishEndpoint, stoppingToken);
            }

            // Periodic cleanup of old published messages
            if (DateTime.UtcNow.Minute == 0) // Run cleanup once per hour
            {
                await outboxRepository.DeleteOldMessagesAsync(_options.RetentionDays, stoppingToken);
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task ProcessSingleMessageAsync(
            OutboxMessage message,
            IOutboxRepository outboxRepository,
            IPublishEndpoint publishEndpoint,
            CancellationToken stoppingToken)
        {
            try
            {
                // Resolve event type - try direct resolution first, then search loaded assemblies
                var eventType = ResolveEventType(message.EventType);
                if (eventType == null)
                {
                    _logger.LogError(
                        "Unknown event type: {EventType}. Message ID: {MessageId}",
                        message.EventType, message.Id);
                    await outboxRepository.MarkAsFailedAsync(
                        message.Id, $"Unknown event type: {message.EventType}", stoppingToken);
                    return;
                }

                // Deserialize event
                var @event = JsonSerializer.Deserialize(message.EventData, eventType, JsonOptions);
                if (@event == null)
                {
                    _logger.LogError(
                        "Failed to deserialize event. Type: {EventType}, Message ID: {MessageId}",
                        message.EventType, message.Id);
                    await outboxRepository.MarkAsFailedAsync(
                        message.Id, "Deserialization returned null", stoppingToken);
                    return;
                }

                // Publish to message broker
                await publishEndpoint.Publish(@event, eventType, stoppingToken);

                // Mark as published
                await outboxRepository.MarkAsPublishedAsync(message.Id, stoppingToken);

                _logger.LogInformation(
                    "Published outbox message. Type: {EventType}, CorrelationId: {CorrelationId}, MessageId: {MessageId}",
                    eventType.Name, message.CorrelationId, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish outbox message. MessageId: {MessageId}, CorrelationId: {CorrelationId}",
                    message.Id, message.CorrelationId);

                await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, stoppingToken);
            }
        }

        /// <summary>
        /// Resolves event type from assembly-qualified name.
        /// First tries Type.GetType, then searches all loaded assemblies.
        /// </summary>
        private static Type? ResolveEventType(string assemblyQualifiedName)
        {
            // Try direct resolution first (works if assembly is already loaded)
            var type = Type.GetType(assemblyQualifiedName);
            if (type != null)
                return type;

            // Extract type name from assembly-qualified name
            // Format: "Namespace.TypeName, AssemblyName, Version=..., Culture=..., PublicKeyToken=..."
            var parts = assemblyQualifiedName.Split(',');
            if (parts.Length < 2)
                return null;

            var fullTypeName = parts[0].Trim();
            var assemblyName = parts[1].Trim();

            // Search all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    type = assembly.GetType(fullTypeName);
                    if (type != null)
                        return type;
                }
            }

            // Try loading the assembly explicitly
            try
            {
                var loadedAssembly = Assembly.Load(assemblyName);
                type = loadedAssembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }
            catch
            {
                // Assembly not found, continue
            }

            // Last resort: search all assemblies by type name only
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(fullTypeName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }

    /// <summary>
    /// Configuration options for the outbox publisher
    /// </summary>
    public class OutboxOptions
    {
        public const string SectionName = "Outbox";

        /// <summary>
        /// How often to poll for pending messages (in seconds). Default: 5
        /// </summary>
        public int PollIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Maximum number of messages to process per batch. Default: 100
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Number of days to keep published messages before cleanup. Default: 7
        /// </summary>
        public int RetentionDays { get; set; } = 7;

        /// <summary>
        /// Enable/disable the outbox publisher. Default: true
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}
