using BackgroundService.Application.Helpers;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;
using Contracts.Events.Notifications.Sms;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    /// <summary>
    /// Processes SMS notifications. Retry policy configured in MassTransit DI.
    /// Failed messages go to sms-notification-queue_error for manual review.
    /// </summary>
    public class SendSmsNotificationConsumer : IConsumer<SendSmsNotificationInternalCommand>
    {
        private readonly ISmsSender _smsSender;
        private readonly ILogger<SendSmsNotificationConsumer> _logger;
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly INotificationLogger _loggerNotification;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public SendSmsNotificationConsumer(
            ISmsSender smsSender,
            ILogger<SendSmsNotificationConsumer> logger,
            INotificationResolverHandler resolverHandler,
            INotificationLogger loggerNotification,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService)
        {
            _smsSender = smsSender;
            _logger = logger;
            _resolverHandler = resolverHandler;
            _loggerNotification = loggerNotification;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
        }

        public async Task Consume(ConsumeContext<SendSmsNotificationInternalCommand> context)
        {
            var msg = context.Message;

            try
            {
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime      = _timeZoneService.GetCurrentTime(systemTimeZoneId);

                // Resolver now returns 15 elements; capture into a variable and read named fields.
                var result = await _resolverHandler.ResolveNotificationTemplatesAsync(
                    msg.UnitId, msg.ModuleName ?? string.Empty, msg.EventTypeId,
                    msg.Email ?? string.Empty, msg.ccMail ?? string.Empty, msg.Mobile ?? string.Empty,
                    msg.param1 ?? string.Empty, msg.param2 ?? string.Empty, msg.param3,
                    msg.param4 ?? string.Empty, msg.param5 ?? string.Empty, msg.param6 ?? string.Empty,
                    msg.param7 ?? string.Empty, msg.param8 ?? string.Empty, msg.param9 ?? string.Empty,
                    msg.param10 ?? string.Empty);

                var smsNumbers = (result.Sms ?? new List<string>())
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();

                if (smsNumbers.Count == 0)
                {
                    await context.Publish(new SendSmsNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "No mobile numbers found"
                    });
                    return;
                }

                var bodyTemplate = result.Body ?? string.Empty;
                var eventRuleId  = result.EventRuleId ?? 0;
                var eventTypeId  = result.EventTypeId ?? 0;
                var channelId    = result.ChannelId ?? (int)NotificationEnum.NotificationChannel.SMS;

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

                // Build final SMS text
                var resolvedBody = TemplateHelper.ReplaceTokens(bodyTemplate, tokens)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(resolvedBody))
                {
                    await context.Publish(new SendSmsNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "Resolved SMS body is empty"
                    });
                    return;
                }

                var sent = await _smsSender.SendSmsAsync(new()
                {
                    mobileNumbers = smsNumbers,
                    message = resolvedBody
                });

                if (sent)
                {
                    _logger.LogInformation("✅ SMS notification sent successfully.");
                    await context.Publish(new SendSmsNotificationCompleted
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
                            ChannelId = channelId,
                            NotificationStatusId = (int)NotificationEnum.NotificationStatus.Success,
                            ReadStatusId = (int)NotificationEnum.NotificationReadStatus.Unread,
                            SendTo = string.Join(",", smsNumbers),
                            ActionStatus = "Sent",
                            MessageText = resolvedBody,
                            Timestamp = currentTime,
                            CreatedBy = int.Parse(_ipAddressService.GetCurrentUserId()),
                            CreatedByName = _ipAddressService.GetUserName(),
                            CreatedIP = _ipAddressService.GetSystemIPAddress(),
                            IsActive = Domain.Common.BaseEntity.Status.Active,
                            IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted,
                            Value= msg.param1
                        });
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogWarning(logEx, "SMS sent but NotificationEventLog write failed (ignored).");
                    }
                }
                else
                {
                    await context.Publish(new SendSmsNotificationFailed
                    {
                        CorrelationId = msg.CorrelationId,
                        Reason = "SMS sending failed"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS Notification Failed for CorrelationId: {CorrelationId}", msg.CorrelationId);

                // Log failure for tracking
                try
                {
                    await _loggerNotification.LogAsync(new NotificationEventLog
                    {
                        NotificationLevelRuleId = null,
                        UnitId = msg.UnitId,
                        ChannelId = (int)NotificationEnum.NotificationChannel.SMS,
                        NotificationStatusId = (int)NotificationEnum.NotificationStatus.Failed,
                        ReadStatusId = (int)NotificationEnum.NotificationReadStatus.Unread,
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
                    _logger.LogWarning(logEx, "Failed to log SMS notification failure");
                }

                // Throw to let MassTransit handle retry with configured policy
                throw;
            }
        }
    }
}
