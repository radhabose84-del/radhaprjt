using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Events.Notifications;
using Contracts.Events.Notifications.Email;
using Contracts.Events.Notifications.InApp;
using Contracts.Events.Notifications.Sms;
using Contracts.Events.Notifications.Whatsapp;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// Orchestrates notification delivery across multiple channels.
    /// Consumes NotificationCreatedEvent from the outbox and dispatches
    /// to channel-specific consumers (Email, SMS, WhatsApp, InApp).
    /// </summary>
    public class NotificationDispatcherConsumer : IConsumer<NotificationCreatedEvent>
    {
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<NotificationDispatcherConsumer> _logger;
        private readonly IInboxRepository _inbox;

        public NotificationDispatcherConsumer(
            INotificationResolverHandler resolverHandler,
            IPublishEndpoint publishEndpoint,
            ILogger<NotificationDispatcherConsumer> logger,
            IInboxRepository inbox)
        {
            _resolverHandler = resolverHandler;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<NotificationCreatedEvent> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(NotificationDispatcherConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}, CorrelationId={CorrelationId}",
                    consumerName, messageId, msg.CorrelationId);
                return;
            }

            _logger.LogInformation(
                "NotificationDispatcher received event. CorrelationId: {CorrelationId}, Module: {Module}, EventType: {EventType}",
                msg.CorrelationId, msg.ModuleName, msg.EventTypeId);

            try
            {
                // Step 1: Resolve which channels are active for this notification
                var channels = await _resolverHandler.ResolveNotificationChannelsAsync(
                    msg.UnitId,
                    msg.ModuleName,
                    msg.EventTypeId,
                    msg.Email ?? string.Empty,
                    msg.ccMail ?? string.Empty,
                    msg.Mobile ?? string.Empty);

                if (channels == null || channels.Count == 0)
                {
                    _logger.LogWarning(
                        "No notification channels configured for Module: {Module}, EventType: {EventType}, UnitId: {UnitId}",
                        msg.ModuleName, msg.EventTypeId, msg.UnitId);
                    return;
                }

                _logger.LogInformation(
                    "Resolved channels for CorrelationId {CorrelationId}: [{Channels}]",
                    msg.CorrelationId, string.Join(", ", channels));

                // Step 2: Convert attachments to the expected format
                var attachments = msg.Attachments?.Select(a => new NotificationCreatedEvent.NotificationAttachment
                {
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    BlobUrl = a.BlobUrl,
                    IsPrivate = a.IsPrivate
                }).ToList() ?? new List<NotificationCreatedEvent.NotificationAttachment>();

                // Step 3: Dispatch to each active channel
                var dispatchTasks = new List<Task>();

                foreach (var channel in channels.Distinct())
                {
                    var normalizedChannel = channel.Trim().ToUpperInvariant();

                    switch (normalizedChannel)
                    {
                        case "EMAIL":
                            dispatchTasks.Add(DispatchEmailAsync(msg, attachments, context.CancellationToken));
                            break;

                        case "SMS":
                            dispatchTasks.Add(DispatchSmsAsync(msg, context.CancellationToken));
                            break;

                        case "WHATSAPP":
                            dispatchTasks.Add(DispatchWhatsAppAsync(msg, context.CancellationToken));
                            break;

                        case "INAPP":
                            dispatchTasks.Add(DispatchInAppAsync(msg, context.CancellationToken));
                            break;

                        default:
                            _logger.LogWarning("Unknown notification channel: {Channel}", channel);
                            break;
                    }
                }

                await Task.WhenAll(dispatchTasks);

                _logger.LogInformation(
                    "NotificationDispatcher completed. CorrelationId: {CorrelationId}, Channels dispatched: {Count}",
                    msg.CorrelationId, dispatchTasks.Count);

                await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "NotificationDispatcher failed. CorrelationId: {CorrelationId}",
                    msg.CorrelationId);
                throw; // Let MassTransit handle retry
            }
        }

        private async Task DispatchEmailAsync(
            NotificationCreatedEvent msg,
            List<NotificationCreatedEvent.NotificationAttachment> attachments,
            CancellationToken cancellationToken)
        {
            var emailCommand = new SendEmailNotificationInternalCommand
            {
                CorrelationId = msg.CorrelationId,
                UnitId = msg.UnitId,
                ModuleName = msg.ModuleName,
                EventTypeId = msg.EventTypeId,
                CreatedByName = msg.CreatedByName,
                Email = msg.Email,
                ccMail = msg.ccMail,
                Mobile = msg.Mobile,
                param1 = msg.param1,
                param2 = msg.param2,
                param3 = msg.param3,
                param4 = msg.param4,
                param5 = msg.param5,
                param6 = msg.param6,
                param7 = msg.param7,
                param8 = msg.param8,
                param9 = msg.param9,
                param10 = msg.TableRowsJson ?? msg.param10,
                RetryCount = 0,
                Attachments = attachments
            };

            await _publishEndpoint.Publish(emailCommand, cancellationToken);
            _logger.LogDebug("Dispatched Email notification. CorrelationId: {CorrelationId}", msg.CorrelationId);
        }

        private async Task DispatchSmsAsync(NotificationCreatedEvent msg, CancellationToken cancellationToken)
        {
            var smsCommand = new SendSmsNotificationInternalCommand
            {
                CorrelationId = msg.CorrelationId,
                UnitId = msg.UnitId,
                ModuleName = msg.ModuleName,
                EventTypeId = msg.EventTypeId,
                CreatedByName = msg.CreatedByName,
                Email = msg.Email,
                ccMail = msg.ccMail,
                Mobile = msg.Mobile,
                param1 = msg.param1,
                param2 = msg.param2,
                param3 = msg.param3,
                param4 = msg.param4,
                param5 = msg.param5,
                param6 = msg.param6,
                param7 = msg.param7,
                param8 = msg.param8,
                param9 = msg.param9,
                param10 = msg.param10,
                RetryCount = 0
            };

            await _publishEndpoint.Publish(smsCommand, cancellationToken);
            _logger.LogDebug("Dispatched SMS notification. CorrelationId: {CorrelationId}", msg.CorrelationId);
        }

        private async Task DispatchWhatsAppAsync(NotificationCreatedEvent msg, CancellationToken cancellationToken)
        {
            var whatsAppCommand = new SendWhatsappNotificationInternalCommand
            {
                CorrelationId = msg.CorrelationId,
                UnitId = msg.UnitId,
                ModuleName = msg.ModuleName,
                EventTypeId = msg.EventTypeId,
                CreatedByName = msg.CreatedByName,
                Email = msg.Email,
                ccMail = msg.ccMail,
                Mobile = msg.Mobile,
                param1 = msg.param1,
                param2 = msg.param2,
                param3 = msg.param3,
                param4 = msg.param4,
                param5 = msg.param5,
                param6 = msg.param6,
                param7 = msg.param7,
                param8 = msg.param8,
                param9 = msg.param9,
                param10 = msg.param10,
                RetryCount = 0
            };

            await _publishEndpoint.Publish(whatsAppCommand, cancellationToken);
            _logger.LogDebug("Dispatched WhatsApp notification. CorrelationId: {CorrelationId}", msg.CorrelationId);
        }

        private async Task DispatchInAppAsync(NotificationCreatedEvent msg, CancellationToken cancellationToken)
        {
            var inAppCommand = new SendInAppNotificationInternalCommand
            {
                CorrelationId = msg.CorrelationId,
                UnitId = msg.UnitId,
                ModuleName = msg.ModuleName,
                EventTypeId = msg.EventTypeId,
                CreatedByName = msg.CreatedByName,
                Email = msg.Email,
                ccMail = msg.ccMail,
                Mobile = msg.Mobile,
                param1 = msg.param1,
                param2 = msg.param2,
                param3 = msg.param3,
                param4 = msg.param4,
                param5 = msg.param5,
                param6 = msg.param6,
                param7 = msg.param7,
                param8 = msg.param8,
                param9 = msg.param9,
                param10 = msg.param10,
                RetryCount = 0
            };

            await _publishEndpoint.Publish(inAppCommand, cancellationToken);
            _logger.LogDebug("Dispatched InApp notification. CorrelationId: {CorrelationId}", msg.CorrelationId);
        }
    }
}
