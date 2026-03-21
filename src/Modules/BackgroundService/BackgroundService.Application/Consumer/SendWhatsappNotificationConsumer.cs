using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackgroundService.Application.Helpers;
using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Interfaces.Lookups.Common;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;
using Contracts.Events.Notifications.Whatsapp;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// Processes WhatsApp notifications. Retry policy configured in MassTransit DI.
    /// Failed messages go to whatsapp-notification-queue_error for manual review.
    /// </summary>
    public class SendWhatsappNotificationConsumer : IConsumer<SendWhatsappNotificationInternalCommand>
    {
        private readonly IWhatsAppSender _waSender;
        private readonly ILogger<SendWhatsappNotificationConsumer> _logger;
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly INotificationLogger _loggerNotification;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IInboxRepository _inbox;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public SendWhatsappNotificationConsumer(
            IWhatsAppSender waSender,
            ILogger<SendWhatsappNotificationConsumer> logger,
            INotificationResolverHandler resolverHandler,
            INotificationLogger loggerNotification,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService,
            IInboxRepository inbox,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _waSender = waSender;
            _logger = logger;
            _resolverHandler = resolverHandler;
            _loggerNotification = loggerNotification;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
            _inbox = inbox;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task Consume(ConsumeContext<SendWhatsappNotificationInternalCommand> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(SendWhatsappNotificationConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}, CorrelationId={CorrelationId}",
                    consumerName, messageId, msg.CorrelationId);
                return;
            }


            // Resolve MiscMaster IDs dynamically
            var channelMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationChannel, NotificationEnum.WhatsApp);
            var successMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationStatus, NotificationEnum.Success);
            var failedMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationStatus, NotificationEnum.Failed);
            var unreadMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationReadStatus, NotificationEnum.Unread);

            try
            {
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

                // ✅ ApiToken now comes from SQL via NotificationResolverHandler (result.ApiToken)
                var result = await _resolverHandler.ResolveNotificationTemplatesAsync(
                    msg.UnitId, msg.ModuleName ?? string.Empty, msg.EventTypeId,
                    msg.Email ?? string.Empty, msg.ccMail ?? string.Empty, msg.Mobile ?? string.Empty,
                    msg.param1 ?? string.Empty, msg.param2 ?? string.Empty, msg.param3,
                    msg.param4 ?? string.Empty, msg.param5 ?? string.Empty, msg.param6 ?? string.Empty,
                    msg.param7 ?? string.Empty, msg.param8 ?? string.Empty, msg.param9 ?? string.Empty,
                    msg.param10 ?? string.Empty);

                // 🔁 Reuse SMS mobile list for WhatsApp
                var waNumbers = (result.Sms ?? new List<string>())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (waNumbers.Count == 0)
                {
                    await context.Publish(new SendWhatsappNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "No WhatsApp mobile numbers found"
                    });
                    return;
                }

                var bodyTemplate = result.Body ?? string.Empty;

                var tokens = new Dictionary<string, string>
                {
                    { "Module",  msg.ModuleName ?? string.Empty },
                    { "param1",  msg.param1 ?? string.Empty },
                    { "param2",  msg.param2 ?? string.Empty },
                    { "param3",  msg.param3.ToString("dd-MMM-yyyy") },
                    { "param4",  msg.param4 ?? string.Empty },
                    { "param5",  msg.param5 ?? string.Empty },
                    { "param6",  msg.param6 ?? string.Empty },
                    { "param7",  msg.param7 ?? string.Empty },
                    { "param8",  msg.param8 ?? string.Empty },
                    { "param9",  msg.param9 ?? string.Empty },
                    { "param10", msg.param10 ?? string.Empty }
                };

                // 1) Replace tokens in HTML
                var htmlBody = TemplateHelper.ReplaceTokens(bodyTemplate, tokens) ?? string.Empty;

                // 2) Convert HTML → plain text for WhatsApp
                var resolvedBody = ToPlainTextAndFormatForWhatsApp(htmlBody);

                if (string.IsNullOrWhiteSpace(resolvedBody))
                {
                    await context.Publish(new SendWhatsappNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "Resolved WhatsApp message body is empty"
                    });
                    return;
                }

                // ✅ Use ApiToken from SQL; do NOT depend on msg.ApiKey
                var apiKey = result.ApiToken ?? string.Empty;
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    // Sender will still fallback to appsettings token if you implemented fallback there
                    _logger.LogWarning(
                        "WhatsApp ApiToken not found from SQL resolver for CorrelationId={CorrelationId}. Sender may fallback to appsettings token.",
                        msg.CorrelationId);
                }

                var sent = await _waSender.SendWhatsAppAsync(new SendWhatsappNotificationCommand
                {
                    MobileNumber = string.Join(",", waNumbers),
                    Message = resolvedBody,
                    ApiKey = apiKey
                });

                if (sent)
                {
                    await context.Publish(new SendWhatsappNotificationCompleted
                    {
                        CorrelationId = msg.CorrelationId
                    });

                    try
                    {
                        await _loggerNotification.LogAsync(new NotificationEventLog
                        {
                            NotificationLevelRuleId = result.EventRuleId,
                            UnitId = msg.UnitId,
                            ChannelId = channelMisc?.Id ?? 0,
                            NotificationStatusId = successMisc?.Id ?? 0,
                            ReadStatusId = unreadMisc?.Id ?? 0,
                            SendTo = string.Join(",", waNumbers),
                            ActionStatus = "Sent",
                            MessageText = resolvedBody,
                            Timestamp = currentTime,
                            CreatedBy = int.Parse(_ipAddressService.GetCurrentUserId()),
                            CreatedByName = _ipAddressService.GetUserName(),
                            CreatedIP = _ipAddressService.GetSystemIPAddress(),
                            IsActive = Domain.Common.BaseEntity.Status.Active,
                            IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted,
                            Value = msg.param1
                        });
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogWarning(logEx,
                            "WhatsApp sent but NotificationEventLog write failed (ignored).");
                    }

                    await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
                }
                else
                {
                    await context.Publish(new SendWhatsappNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "WhatsApp sending failed"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WhatsApp Notification Failed for CorrelationId: {CorrelationId}", msg.CorrelationId);

                // Log failure for tracking
                try
                {
                    await _loggerNotification.LogAsync(new NotificationEventLog
                    {
                        NotificationLevelRuleId = null,
                        UnitId = msg.UnitId,
                        ChannelId = channelMisc?.Id ?? 0,
                        NotificationStatusId = failedMisc?.Id ?? 0,
                        ReadStatusId = unreadMisc?.Id ?? 0,
                        SendTo = msg.Mobile ?? "Unknown",
                        ActionStatus = "Failed",
                        MessageText = ex.Message,
                        Timestamp = DateTimeOffset.UtcNow,
                        CreatedBy = _ipAddressService.GetUserId(),
                        CreatedByName = _ipAddressService.GetUserName(),
                        CreatedIP = _ipAddressService.GetSystemIPAddress(),
                        IsActive = Domain.Common.BaseEntity.Status.Active,
                        IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted
                    });
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Failed to log WhatsApp notification failure");
                }

                // Throw to let MassTransit handle retry with configured policy
                throw;
            }
        }

        private static string ToPlainTextAndFormatForWhatsApp(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            // <br>, </p> => newline
            html = Regex.Replace(html, @"<\s*br\s*/?\s*>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</\s*p\s*>", "\n", RegexOptions.IgnoreCase);

            // remove heading tags but do NOT add newlines
            html = Regex.Replace(html, @"<\s*h[1-6][^>]*>", string.Empty, RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</\s*h[1-6]\s*>", string.Empty, RegexOptions.IgnoreCase);

            // remove any remaining tags
            html = Regex.Replace(html, @"<[^>]+>", string.Empty);

            // decode entities
            html = WebUtility.HtmlDecode(html);

            // normalize newlines / spaces
            html = Regex.Replace(html, @"\r", "");
            html = Regex.Replace(html, @"[ \t]+\n", "\n");
            html = Regex.Replace(html, @"\n{2,}", "\n\n"); // at most one blank line
            html = html.Trim();

            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Bold the first non-empty line for WhatsApp
            var lines = html.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    var trimmed = lines[i].Trim();
                    if (!trimmed.StartsWith("*") && !trimmed.EndsWith("*"))
                        lines[i] = $"*{trimmed}*";
                    break;
                }
            }

            return string.Join("\n", lines);
        }
    }
}
