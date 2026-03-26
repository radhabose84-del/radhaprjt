#nullable disable
using Microsoft.AspNetCore.SignalR.Client;

namespace BSOFT.Worker.Services;

public sealed class SignalRWorkerNotificationService : IWorkerNotificationService, IAsyncDisposable
{
    private readonly ILogger<SignalRWorkerNotificationService> _logger;
    private readonly HubConnection _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public SignalRWorkerNotificationService(
        IConfiguration configuration,
        ILogger<SignalRWorkerNotificationService> logger)
    {
        _logger = logger;

        var hubUrl = configuration["SignalR:HubUrl"] ?? "http://localhost:5050/notificationHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task SendAsync(string method, object payload)
    {
        try
        {
            await EnsureConnectedAsync();
            await _connection.InvokeAsync("BroadcastFromWorker", method, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification. Method: {Method}", method);
        }
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return;

        await _connectionLock.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                _logger.LogInformation("SignalR connection established to hub.");
            }

            // Wait for connection if it's in Connecting/Reconnecting state
            var timeout = TimeSpan.FromSeconds(10);
            var start = DateTime.UtcNow;
            while (_connection.State != HubConnectionState.Connected
                && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(100);
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
        _connectionLock.Dispose();
    }
}
