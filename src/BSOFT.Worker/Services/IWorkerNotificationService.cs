namespace BSOFT.Worker.Services;

public interface IWorkerNotificationService
{
    /// <summary>
    /// Sends a notification to a specific user via their SignalR group.
    /// </summary>
    Task SendToUserAsync(string userId, string method, object payload);

    /// <summary>
    /// Broadcasts a notification to ALL connected clients (system-wide alerts).
    /// </summary>
    Task BroadcastAsync(string method, object payload);
}
