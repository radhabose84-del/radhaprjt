using BackgroundService.Application.DTO;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Services.Notification
{
    public class InAppNotifier : IInAppNotifier
    {
        private readonly ILogger<InAppNotifier> _logger;
        private readonly INotificationLogger _loggerNotification;
        private readonly ITimeZoneService _timeZoneService;

        public InAppNotifier(
            ILogger<InAppNotifier> logger,
            INotificationLogger loggerNotification,
            ITimeZoneService timeZoneService)
        {
            _logger = logger;
            _loggerNotification = loggerNotification;
            _timeZoneService = timeZoneService;
        }

        public async Task<bool> SendInAppNotificationAsync(
            List<int> userIds,
            string message,
            string title,
            string value,
            NotificationContext context)
        {
            if (userIds is null || userIds.Count == 0)
            {
                _logger.LogWarning("No users provided for in-app notification.");
                return false;
            }

            try
            {
                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

                foreach (var id in userIds)
                {
                    var log = new NotificationEventLog
                    {
                        NotificationLevelRuleId = context.EventRuleId,
                        NotificationStatusId    = (int)NotificationEnum.NotificationStatus.Success,
                        ReadStatusId            = (int)NotificationEnum.NotificationReadStatus.Unread,
                        SendTo                  = id.ToString(),
                        ActionStatus            = "Sent",
                        ChannelId               = (int)NotificationEnum.NotificationChannel.InApp,
                        MessageText             = message,
                        Timestamp               = currentTime,
                        CreatedBy               = context.CreatedById,
                        CreatedDate             = DateTime.UtcNow,
                        CreatedByName           = context.CreatedByName,
                        CreatedIP               = context.CreatedIp,
                        UnitId                  = context.UnitId ?? 0,
                        Value                   = value
                    };

                    await _loggerNotification.LogAsync(log);
                    // SignalR push is handled by WorkerInAppNotifier in BSOFT.Worker,
                    // which overrides this class and uses IWorkerNotificationService.
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send in-app notification to users");
                return false;
            }
        }
    }
}
