#nullable disable
using BackgroundService.Application.DTO;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;

namespace BSOFT.Worker.Services;

/// <summary>
/// Worker-aware implementation of <see cref="IInAppNotifier"/>.
/// Unlike <c>InAppNotifier</c> (which uses IHubContext&lt;NotificationHub&gt; and therefore only
/// reaches Angular clients that have connected directly to the Worker process),
/// this implementation pushes notifications through <see cref="IWorkerNotificationService"/>
/// — a SignalR CLIENT that relays the payload to the hub hosted in BSOFT.Api,
/// where real Angular clients are connected.
/// </summary>
internal sealed class WorkerInAppNotifier : IInAppNotifier
{
    private readonly ILogger<WorkerInAppNotifier> _logger;
    private readonly IWorkerNotificationService _workerNotificationService;
    private readonly INotificationLogger _notificationLogger;
    private readonly ITimeZoneService _timeZoneService;

    public WorkerInAppNotifier(
        ILogger<WorkerInAppNotifier> logger,
        IWorkerNotificationService workerNotificationService,
        INotificationLogger notificationLogger,
        ITimeZoneService timeZoneService)
    {
        _logger = logger;
        _workerNotificationService = workerNotificationService;
        _notificationLogger = notificationLogger;
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
                // 1. Persist the notification event log to SQL Server (same as InAppNotifier)
                var log = new NotificationEventLog
                {
                    NotificationLevelRuleId = context.EventRuleId,
                    NotificationStatusId = (int)NotificationEnum.NotificationStatus.Success,
                    ReadStatusId = (int)NotificationEnum.NotificationReadStatus.Unread,
                    SendTo = id.ToString(),
                    ActionStatus = "Sent",
                    ChannelId = (int)NotificationEnum.NotificationChannel.InApp,
                    MessageText = message,
                    Timestamp = currentTime,
                    CreatedBy = context.CreatedById,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByName = context.CreatedByName,
                    CreatedIP = context.CreatedIp,
                    UnitId = context.UnitId ?? 0,
                    Value = value
                };

                var savedLogId = await _notificationLogger.LogAsync(log);

                // 2. Push to Angular via SignalR client → BSOFT.Api hub → Angular
                //    (IHubContext<NotificationHub> cannot be used here — Angular clients
                //     connect to BSOFT.Api's hub, not to the Worker process)
                await _workerNotificationService.SendAsync("ReceiveNotification", new
                {
                    LogId = savedLogId,
                    Message = message,
                    Title = title,
                    Timestamp = currentTime,
                    Type = "info",
                    UserId = id.ToString(),
                    Value = value
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WorkerInAppNotifier: Failed to send in-app notification to users.");
            return false;
        }
    }
}
