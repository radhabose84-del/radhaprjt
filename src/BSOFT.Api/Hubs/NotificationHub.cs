#nullable disable
using Microsoft.AspNetCore.SignalR;

namespace BSOFT.Api.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string userId, string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
    public async Task BroadcastFromWorker(string method, object payload)
    {
        await Clients.All.SendAsync(method, payload);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }
}
