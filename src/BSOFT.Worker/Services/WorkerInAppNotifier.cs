using BackgroundService.Application.DTO;
using Contracts.Interfaces.Lookups.Common;
using BackgroundService.Application.Interfaces.Notification;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Contracts.Events.Notifications;

namespace BSOFT.Worker.Services;

/// <summary>
/// Worker-aware implementation of <see cref="IInAppNotifier"/>.
/// Pushes notifications through <see cref="IWorkerNotificationService"/>
/// — a SignalR CLIENT that relays the payload to the hub hosted in BSOFT.Api,
/// where real Angular clients are connected.
///
/// Reliability strategy:
/// 1. ALWAYS save to DB first (source of truth) — this never fails silently
/// 2. THEN push via SignalR (best-effort per user) — isolated failures per user
/// 3. If SignalR fails for a user, the DB log still exists — Angular can fetch on reconnect
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

        var totalUsers = userIds.Count;
        var dbSaved = 0;
        var signalRSent = 0;
        var signalRFailed = 0;

        foreach (var userId in userIds)
        {
            // ── Step 1: ALWAYS save to DB first (source of truth) ──
            int savedLogId;
            try
            {
                var log = new NotificationEventLog
                {
                    NotificationLevelRuleId = context.EventRuleId,
                    NotificationStatusId = successMisc?.Id ?? 0,
                    ReadStatusId = unreadMisc?.Id ?? 0,
                    SendTo = userId.ToString(),
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
                _logger.LogError(ex,
                    "WorkerInAppNotifier: Failed to save notification log for user {UserId}. Skipping SignalR push.",
                    userId);
                continue; // Skip SignalR for this user — DB is source of truth
            }

            // ── Step 2: Push via SignalR (best-effort, isolated per user) ──
            try
            {
                await _workerNotificationService.SendToUserAsync(userId.ToString(), "ReceiveNotification", new
                {
                    LogId = savedLogId,
                    Message = message,
                    Title = title,
                    Timestamp = currentTime,
                    Type = "info",
                    UserId = userId.ToString(),
                    Value = value
                });
                signalRSent++;
            }
            catch (Exception ex)
            {
                signalRFailed++;
                _logger.LogWarning(ex,
                    "WorkerInAppNotifier: SignalR push failed for user {UserId} (LogId={LogId}). " +
                    "Notification saved to DB — user can fetch on reconnect.",
                    userId, savedLogId);
                // Do NOT throw — continue to next user
            }
        }

        _logger.LogInformation(
            "WorkerInAppNotifier: Completed. Users={Total}, DB_Saved={DbSaved}, SignalR_Sent={Sent}, SignalR_Failed={Failed}",
            totalUsers, dbSaved, signalRSent, signalRFailed);

        // Return true as long as at least one DB save succeeded
        // (SignalR failures are acceptable — DB is the source of truth)
        return dbSaved > 0;
    }
}
