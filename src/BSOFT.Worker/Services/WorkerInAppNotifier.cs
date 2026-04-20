using BackgroundService.Application.DTO;
using Contracts.Interfaces.Lookups.Common;
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
    private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

    public WorkerInAppNotifier(
        ILogger<WorkerInAppNotifier> logger,
        IWorkerNotificationService workerNotificationService,
        INotificationLogger notificationLogger,
        ITimeZoneService timeZoneService,
        IAppDataMiscMasterLookup appDataMiscLookup)
    {
        _logger = logger;
        _workerNotificationService = workerNotificationService;
        _notificationLogger = notificationLogger;
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

        // Resolve MiscMaster IDs dynamically
        var successMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationStatus, NotificationEnum.Success);
        var unreadMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationReadStatus, NotificationEnum.Unread);
        var channelMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationChannel, NotificationEnum.InApp);

        var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
        var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

        var dbSaved = 0;
        var signalRSent = 0;

        // FIX #4: Per-user isolation — one user's failure does NOT block others
        foreach (var id in userIds)
        {
            // Step 1: ALWAYS save to DB first (source of truth)
            int savedLogId;
            try
            {
                var log = new NotificationEventLog
                {
                    NotificationLevelRuleId = context.EventRuleId,
                    NotificationStatusId = successMisc?.Id ?? 0,
                    ReadStatusId = unreadMisc?.Id ?? 0,
                    SendTo = id.ToString(),
                    ActionStatus = "Sent",
                    ChannelId = channelMisc?.Id ?? 0,
                    MessageText = message,
                    Timestamp = currentTime,
                    CreatedBy = context.CreatedById,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByName = context.CreatedByName,
                    CreatedIP = context.CreatedIp,
                    UnitId = context.UnitId ?? 0,
                    Value = value
                };

                savedLogId = await _notificationLogger.LogAsync(log);
                dbSaved++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WorkerInAppNotifier: DB save failed for user {UserId}.", id);
                continue; // Skip SignalR — but continue to next user
            }

            // Step 2: Push via SignalR (best-effort per user)
            try
            {
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
                signalRSent++;
            }
            catch (Exception ex)
            {
                // SignalR failed for THIS user only — DB log exists, user can fetch on reconnect
                _logger.LogWarning(ex,
                    "WorkerInAppNotifier: SignalR push failed for user {UserId} (LogId={LogId}). DB log saved.",
                    id, savedLogId);
            }
        }

        if (dbSaved < userIds.Count || signalRSent < dbSaved)
        {
            _logger.LogWarning(
                "WorkerInAppNotifier: Partial failure — Users={Total}, DB_Saved={DbSaved}, SignalR_Sent={Sent}, DB_Failed={DbFailed}, SignalR_Failed={SignalRFailed}",
                userIds.Count, dbSaved, signalRSent, userIds.Count - dbSaved, dbSaved - signalRSent);
        }
        else
        {
            _logger.LogInformation(
                "WorkerInAppNotifier: Users={Total}, DB_Saved={DbSaved}, SignalR_Sent={Sent}",
                userIds.Count, dbSaved, signalRSent);
        }

        return dbSaved > 0;
    }
}
