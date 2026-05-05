using BackgroundService.Application.DTO;
using BackgroundService.Application.Helpers;
using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Interfaces.Lookups.Common;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Interfaces;
using Contracts.Events.Notifications;
using Contracts.Events.Notifications.InApp;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// Processes in-app notifications. Retry policy configured in MassTransit DI.
    /// Failed messages go to inapp-notification-queue_error for manual review.
    /// </summary>
    public class SendInAppNotificationConsumer : IConsumer<SendInAppNotificationInternalCommand>
    {
        private readonly IInAppNotifier _notifier;
        private readonly ILogger<SendInAppNotificationConsumer> _logger;
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly IIPAddressService _ipAddressService;
        private readonly IInboxRepository _inbox;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public SendInAppNotificationConsumer(
            IInAppNotifier notifier,
            ILogger<SendInAppNotificationConsumer> logger,
            INotificationResolverHandler resolverHandler,
            IIPAddressService ipAddressService,
            IInboxRepository inbox,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _notifier = notifier;
            _logger = logger;
            _resolverHandler = resolverHandler;
            _ipAddressService = ipAddressService;
            _inbox = inbox;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task Consume(ConsumeContext<SendInAppNotificationInternalCommand> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(SendInAppNotificationConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}, CorrelationId={CorrelationId}",
                    consumerName, messageId, msg.CorrelationId);
                return;
            }

            // Resolve MiscMaster IDs dynamically
            var channelMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationChannel, NotificationEnum.InApp);

            try
            {
                // The first override user id (if set) is passed to the SP as @UserId so the SP can
                // resolve AGENT_MO_USER / AGENT_MO_REPORTS_TO target types via the Users table.
                var userIdParam = msg.OverrideTargetUserIds is { Count: > 0 }
                    ? msg.OverrideTargetUserIds[0].ToString()
                    : string.Empty;

                var (_, _, _, _, userIds, subject, header, body, footer, lang,
                     eventTypeId, eventRuleId, channelId, _templateId, _isTable,apiToken)
                    = await _resolverHandler.ResolveNotificationTemplatesAsync(
                        msg.UnitId, msg.ModuleName, msg.EventTypeId,
                        msg.Email ?? "", msg.ccMail ?? "", msg.Mobile ?? "",
                        userIdParam,
                        msg.param1, msg.param2, msg.param3,
                        msg.param4, msg.param5, msg.param6, msg.param7, msg.param8, msg.param9, msg.param10);

                // Caller-supplied recipient override — used by handlers that resolve a specific user
                // in code (e.g., Sales Order → Agent's Marketing Officer). Templates still come from
                // the SP above; only the recipient list is replaced.
                if (msg.OverrideTargetUserIds is { Count: > 0 })
                {
                    _logger.LogInformation(
                        "InApp recipient override applied. CorrId={CorrelationId}, OverrideUsers={OverrideCount}, SpResolvedUsers={SpCount}",
                        msg.CorrelationId, msg.OverrideTargetUserIds.Count, userIds?.Count ?? 0);
                    userIds = msg.OverrideTargetUserIds;
                }

                if (userIds == null || userIds.Count == 0)
                {
                    _logger.LogWarning("No in-app users found to notify. CorrId={CorrelationId}", msg.CorrelationId);
                    await context.Publish(new SendInAppNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "No in-app user IDs resolved"
                    });
                    // Mark as processed to prevent infinite retries when no users are configured
                    await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
                    return;
                }

                var tokens = new Dictionary<string, string>
                {
                    { "Module",  msg.ModuleName ?? "" },
                    { "param1",  msg.param1 ?? "" },
                    { "param2",  msg.param2 ?? "" },
                    { "param3",  msg.param3.ToString(NotificationEnum.DateFormat) },
                    { "param4",  msg.param4 ?? "" },
                    { "param5",  msg.param5 ?? "" },
                    { "param6",  msg.param6 ?? "" },
                    { "param7",  msg.param7 ?? "" },
                    { "param8",  msg.param8 ?? "" },
                    { "param9",  msg.param9 ?? "" },
                    { "param10", msg.param10 ?? "" }
                };

                // ✅ Title = resolved subject (fallback to header), Body = resolved body
                string resolvedTitle = TemplateHelper.ReplaceTokens(!string.IsNullOrWhiteSpace(subject) ? subject : (header ?? "Notification"), tokens);
                string resolvedBody = TemplateHelper.ReplaceTokens(body ?? string.Empty, tokens);

                var contextData = new NotificationContext
                {
                    EventRuleId = eventRuleId ?? 0,
                    EventTypeId = eventTypeId ?? 0,
                    ChannelId = channelMisc?.Id ?? 0,
                    CreatedById = int.TryParse(_ipAddressService.GetCurrentUserId(), out var uid) ? uid : 0,
                    CreatedByName = _ipAddressService.GetUserName(),
                    CreatedIp = _ipAddressService.GetSystemIPAddress(),
                    UnitId = msg.UnitId
                };

                var sent = await _notifier.SendInAppNotificationAsync(userIds, resolvedBody, resolvedTitle, msg.param1, contextData);

                if (sent)
                {
                    _logger.LogInformation("In-app notification sent. CorrId={CorrelationId}", msg.CorrelationId);
                    await context.Publish(new SendInAppNotificationCompleted
                    {
                        CorrelationId = msg.CorrelationId
                    });
                    await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
                }
                else
                {
                    throw new Exception("In-app notifier returned false.");
                }
                _logger.LogInformation("InApp targets: {Count} users, CorrId={CorrelationId}",
                    userIds?.Count ?? 0, msg.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending in-app notification. CorrId={CorrelationId}", msg.CorrelationId);

                // Throw to let MassTransit handle retry with configured policy
                throw;
            }
        }
    }
}
