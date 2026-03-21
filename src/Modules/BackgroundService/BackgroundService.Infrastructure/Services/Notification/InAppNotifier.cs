using BackgroundService.Application.DTO;
using BackgroundService.Application.Interfaces;
using Contracts.Interfaces.Lookups.Common;
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
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public InAppNotifier(
            ILogger<InAppNotifier> logger,
            INotificationLogger loggerNotification,
            ITimeZoneService timeZoneService,
            IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _logger = logger;
            _loggerNotification = loggerNotification;
            _timeZoneService = timeZoneService;
            _appDataMiscLookup = appDataMiscLookup;
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
                // Resolve MiscMaster IDs dynamically
                var successMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationStatus, NotificationEnum.Success);
                var unreadMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationReadStatus, NotificationEnum.Unread);
                var channelMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationChannel, NotificationEnum.InApp);

                var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
                var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

                foreach (var id in userIds)
                {
                    var log = new NotificationEventLog
                    {
                        NotificationLevelRuleId = context.EventRuleId,
                        NotificationStatusId    = successMisc?.Id ?? 0,
                        ReadStatusId            = unreadMisc?.Id ?? 0,
                        SendTo                  = id.ToString(),
                        ActionStatus            = "Sent",
                        ChannelId               = channelMisc?.Id ?? 0,
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
