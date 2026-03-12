using System;
using System.Collections.Generic; // ✅ Needed for Dictionary<>
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BackgroundService.Application.Helpers;
using BackgroundService.Application.Interfaces.Files;
using BackgroundService.Application.Interfaces.IInbox;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;
using Contracts.Events.Notifications.Email;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// Processes email notifications. Retry policy configured in MassTransit DI.
    /// Failed messages go to email-notification-queue_error for manual review.
    /// </summary>
    public class SendEmailNotificationConsumer : IConsumer<SendEmailNotificationInternalCommand>
    {
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SendEmailNotificationConsumer> _logger;
        private readonly INotificationLogger _loggerNotification;
        private readonly IIPAddressService _ipAddressService;
        private readonly IBackgroundJobClient _jobClient;
        private readonly ITimeZoneService _timeZoneService;

        // SQL-based renderer
        private readonly IHtmlTableRenderer _htmlTableRenderer;
        private readonly IFileFetcher _fileFetcher;
        private readonly IInboxRepository _inbox;

        public SendEmailNotificationConsumer(
            INotificationResolverHandler resolverHandler,
            IEmailSender emailSender,
            ILogger<SendEmailNotificationConsumer> logger,
            INotificationLogger loggerNotification,
            IIPAddressService ipAddressService,
            IBackgroundJobClient jobClient,
            ITimeZoneService timeZoneService,
            IHtmlTableRenderer htmlTableRenderer,
            IFileFetcher fileFetcher,
            IInboxRepository inbox)
        {
            _resolverHandler = resolverHandler;
            _emailSender = emailSender;
            _logger = logger;
            _loggerNotification = loggerNotification;
            _ipAddressService = ipAddressService;
            _jobClient = jobClient;
            _timeZoneService = timeZoneService;
            _htmlTableRenderer = htmlTableRenderer;
            _fileFetcher = fileFetcher;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<SendEmailNotificationInternalCommand> context)
        {
            var msg = context.Message;
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(SendEmailNotificationConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}, CorrelationId={CorrelationId}",
                    consumerName, messageId, msg.CorrelationId);
                return;
            }

            try
            {
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);
                 // ✅ Build attachments only if provided
                List<(string FileName, string ContentType, byte[] Data)>? emailAttachments = null;

                if (msg.Attachments is { Count: > 0 })
                {
                    emailAttachments = new();
                    foreach (var a in msg.Attachments)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(a.BlobUrl)) continue;
                            var bytes = await _fileFetcher.FetchAsync(a.BlobUrl, context.CancellationToken);
                            if (bytes?.Length > 0)
                                emailAttachments.Add((a.FileName, a.ContentType, bytes));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to fetch attachment: {Url}", a.BlobUrl);
                        }
                    }
                    if (emailAttachments.Count == 0) emailAttachments = null;
                }

                var r = await _resolverHandler.ResolveNotificationTemplatesAsync(
                    msg.UnitId, msg.ModuleName, msg.EventTypeId,
                    msg.Email ?? "", msg.ccMail ?? "", msg.Mobile ?? "",
                    msg.param1, msg.param2, msg.param3,
                    msg.param4, msg.param5, msg.param6, msg.param7, msg.param8, msg.param9, msg.param10, emailAttachments);

                var toEmails    = r.To;
                var ccEmails    = r.Cc;
                var bccEmails   = r.Bcc;
                var subjectT    = r.Subject;
                var headerT     = r.Header;
                var bodyT       = r.Body;
                var footerT     = r.Footer;
                var eventTypeId = r.EventTypeId;
                var eventRuleId = r.EventRuleId;
                var channelId   = r.ChannelId;
                var templateId  = r.TemplateId;
                var isTable     = r.IsTable;

                if (toEmails is null || toEmails.Count == 0)
                {
                    _logger.LogWarning("No recipients for CorrelationId: {CorrelationId}", msg.CorrelationId);
                    await context.Publish(new SendEmailNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "No recipients found"
                    });
                    return;
                }

                _logger.LogInformation("Email template: TemplateId={TemplateId}, IsTable={IsTable}, HasRows={HasRows}",
                    templateId, isTable, !string.IsNullOrWhiteSpace(msg.param10));

                // 1) Render {param4} via SQL if template expects table (or rows JSON present)
                string? param4Html = msg.param4;
                var wantsTable = isTable || !string.IsNullOrWhiteSpace(msg.param10);
                if (wantsTable && string.IsNullOrWhiteSpace(param4Html))
                {
                    if (templateId <= 0)
                    {
                        _logger.LogWarning("IsTable is true but TemplateId is 0. Cannot render table.");
                    }
                    else if (string.IsNullOrWhiteSpace(msg.param10))
                    {
                        _logger.LogWarning("IsTable is true but param10 (rows JSON) is empty.");
                    }
                    else
                    {
                        param4Html = await _htmlTableRenderer.RenderFromTemplateAsync(
                            templateId, msg.param10, context.CancellationToken);

                        if (string.IsNullOrWhiteSpace(param4Html))
                            _logger.LogWarning("SQL renderer returned empty HTML for TemplateId={TemplateId}.", templateId);
                    }
                }

                // 2) Tokens AFTER we have param4Html
                var tokens = new Dictionary<string, string>
                {
                    { "Module",  msg.ModuleName ?? "" },
                    { "param1",  msg.param1 ?? "" },
                    { "param2",  msg.param2 ?? "" },
                    { "param3",  msg.param3.ToString("dd-MMM-yyyy") },
                    { "param4",  param4Html ?? "" }, // rendered table
                    { "param5",  msg.param5 ?? "" },
                    { "param6",  msg.param6 ?? "" },
                    { "param7",  msg.param7 ?? "" },
                    { "param8",  msg.param8 ?? "" },
                    { "param9",  msg.param9 ?? "" },
                    { "param10", msg.param10 ?? "" }
                };

                // 3) Replace tokens (gentle cleanup; do not strip valid table HTML)
                var resolvedSubject = TemplateHelper.ReplaceTokens(subjectT ?? "", tokens);
                var resolvedHeader  = NormalizeTemplate(TemplateHelper.ReplaceTokens(headerT ?? "", tokens));
                var resolvedBody    = NormalizeTemplate(TemplateHelper.ReplaceTokens(bodyT ?? "", tokens));
                var resolvedFooter  = NormalizeTemplate(TemplateHelper.ReplaceTokens(footerT ?? "", tokens));
          
                // 4) Send
                var success = await _emailSender.SendEmailAsync(
                    toEmails, resolvedSubject, resolvedHeader, resolvedBody, resolvedFooter,
                    ccEmails, bccEmails, channelId ?? 0, eventRuleId ?? 0, eventTypeId ?? 0, attachments:emailAttachments );

                if (success)
                {
                    await context.Publish(new SendEmailNotificationCompleted
                    {
                        CorrelationId = msg.CorrelationId
                    });

                    // Best-effort log
                    try
                    {
                        await _loggerNotification.LogAsync(new NotificationEventLog
                        {
                            NotificationLevelRuleId = eventRuleId,
                            UnitId = msg.UnitId,
                            ChannelId = channelId ?? (int)NotificationEnum.NotificationChannel.Email,
                            NotificationStatusId = (int)NotificationEnum.NotificationStatus.Success,
                            ReadStatusId = (int)NotificationEnum.NotificationReadStatus.Unread,
                            SendTo = string.Join(",", toEmails),
                            ActionStatus = "Sent",
                            MessageText = $"{resolvedHeader}{resolvedBody}{resolvedFooter}",
                            Timestamp = currentTime,
                            CreatedBy = int.Parse(_ipAddressService.GetCurrentUserId()),
                            CreatedByName = _ipAddressService.GetUserName(),
                            CreatedIP = _ipAddressService.GetSystemIPAddress(),
                            IsActive = BackgroundService.Domain.Common.BaseEntity.Status.Active,
                            IsDeleted = BackgroundService.Domain.Common.BaseEntity.IsDelete.NotDeleted,
                            Value= msg.param1
                        });
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogWarning(logEx, "Message logged as sent, but NotificationEventLog write failed.");
                    }

                    await _inbox.MarkAsProcessedAsync(consumerName, messageId, msg.CorrelationId, context.CancellationToken);
                }
                else
                {
                    throw new Exception("Email sender returned false.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email notification failed (CorrelationId: {CorrelationId})", msg.CorrelationId);

                // Log failure for tracking
                try
                {
                    await _loggerNotification.LogAsync(new NotificationEventLog
                    {
                        NotificationLevelRuleId = null,
                        UnitId = msg.UnitId,
                        NotificationStatusId = (int)NotificationEnum.NotificationStatus.Failed,
                        ReadStatusId = (int)NotificationEnum.NotificationReadStatus.Unread,
                        SendTo = msg.Email ?? "Unknown",
                        ActionStatus = "Failed",
                        ChannelId = (int)NotificationEnum.NotificationChannel.Email,
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
                    _logger.LogWarning(logEx, "Failed to log notification failure");
                }

                // Throw to let MassTransit handle retry with configured policy
                throw;
            }
        }

        // Gentle cleanup only; do not break table HTML
        private static string NormalizeTemplate(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;

            html = html.Replace("<n>", "<br/>", StringComparison.OrdinalIgnoreCase)
                       .Replace("</n>", "", StringComparison.OrdinalIgnoreCase)
                       .Replace("<strorng>", "<strong>", StringComparison.OrdinalIgnoreCase)
                       .Replace("<s/trong>", "</strong>", StringComparison.OrdinalIgnoreCase);

            html = Regex.Replace(html, @"<\s+", "<");   // "<  p"  -> "<p"
            html = Regex.Replace(html, @"\s+>", ">");   // "p  >"  -> "p>"
            html = Regex.Replace(html, @"</\s+", "</"); // "</  p" -> "</p>"

            return html;
        }
    }
}
