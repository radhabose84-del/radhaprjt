using Microsoft.AspNetCore.SignalR;

namespace BSOFT.Api.Hubs;

public class NotificationHub : Hub
{
    /// <summary>
    /// Called by Angular clients after connecting to join their user-specific group.
    /// Usage from Angular: hubConnection.invoke('JoinUserGroup', userId.toString());
    /// </summary>
    public async Task JoinUserGroup(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
            Console.WriteLine($"Client {Context.ConnectionId} joined group user-{userId}");
        }
    }

    /// <summary>
    /// Called by Angular clients to leave their group (e.g., on logout).
    /// </summary>
    public async Task LeaveUserGroup(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
    }

    /// <summary>
    /// Called by the Worker's SignalR client to relay a notification to a specific user group.
    /// </summary>
    public async Task SendToUser(string userId, string method, object payload)
    {
        await Clients.Group($"user-{userId}").SendAsync(method, payload);
    }

    /// <summary>
    /// Called by the Worker's SignalR client to broadcast a notification to ALL connected clients.
    /// Use only when all users must see the notification (system-wide alerts).
    /// </summary>
    public async Task BroadcastFromWorker(string method, object payload)
    {
        await Clients.All.SendAsync(method, payload);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}
