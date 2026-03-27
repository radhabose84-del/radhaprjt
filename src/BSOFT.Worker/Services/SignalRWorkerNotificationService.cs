using Microsoft.AspNetCore.SignalR.Client;

namespace BSOFT.Worker.Services;

/// <summary>
/// SignalR client that connects from BSOFT.Worker to the NotificationHub hosted in BSOFT.Api.
/// Registered as Singleton — maintains a single persistent connection with automatic reconnect.
/// </summary>
public sealed class SignalRWorkerNotificationService : IWorkerNotificationService, IAsyncDisposable
{
    private readonly ILogger<SignalRWorkerNotificationService> _logger;
    private readonly HubConnection _connection;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public SignalRWorkerNotificationService(
        IConfiguration configuration,
        ILogger<SignalRWorkerNotificationService> logger)
    {
        _logger = logger;

        var hubUrl = configuration["SignalR:HubUrl"] ?? "http://localhost:5050/notificationHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)
            })
            .Build();

        _connection.Closed += async (error) =>
        {
            _logger.LogWarning(error, "SignalR connection closed. Will attempt reconnect.");
            await Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            _logger.LogInformation("SignalR reconnecting... {Error}", error?.Message);
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected. ConnectionId: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        // Fire-and-forget startup connection — don't block DI
        _ = ConnectWithRetryAsync();
    }

    /// <summary>
    /// Sends a notification to a specific user's SignalR group via the hub's SendToUser method.
    /// </summary>
    public async Task SendToUserAsync(string userId, string method, object payload)
    {
        if (_disposed) return;

        try
        {
            await EnsureConnectedAsync();
            await _connection.InvokeAsync("SendToUser", userId, method, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send SignalR notification to user {UserId}. Method: {Method}",
                userId, method);
            throw; // Let caller decide how to handle
        }
    }

    /// <summary>
    /// Broadcasts a notification to ALL connected clients via the hub's BroadcastFromWorker method.
    /// </summary>
    public async Task BroadcastAsync(string method, object payload)
    {
        if (_disposed) return;

        try
        {
            await EnsureConnectedAsync();
            await _connection.InvokeAsync("BroadcastFromWorker", method, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast SignalR notification. Method: {Method}", method);
            throw;
        }
    }

    /// <summary>
    /// Ensures the hub connection is active. Uses a semaphore to prevent concurrent connect attempts.
    /// </summary>
    private async Task EnsureConnectedAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return;

        await _connectionLock.WaitAsync(TimeSpan.FromSeconds(15));
        try
        {
            if (_connection.State == HubConnectionState.Connected)
                return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                _logger.LogInformation("SignalR connection established to hub.");
            }
            else
            {
                // Connecting or Reconnecting — wait briefly
                var sw = System.Diagnostics.Stopwatch.StartNew();
                while (_connection.State != HubConnectionState.Connected && sw.Elapsed < TimeSpan.FromSeconds(10))
                {
                    await Task.Delay(200);
                }

                if (_connection.State != HubConnectionState.Connected)
                {
                    throw new InvalidOperationException(
                        $"SignalR connection stuck in {_connection.State} state after 10s timeout.");
                }
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Attempts to connect at startup with retry. Runs in background so DI doesn't block.
    /// </summary>
    private async Task ConnectWithRetryAsync()
    {
        var delays = new[] { 1, 2, 5, 10, 30 };
        for (var i = 0; i < delays.Length; i++)
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    _logger.LogInformation("SignalR startup connection established to hub.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    "SignalR startup connect attempt {Attempt}/{Max} failed: {Error}. Retrying in {Delay}s.",
                    i + 1, delays.Length, ex.Message, delays[i]);
                await Task.Delay(TimeSpan.FromSeconds(delays[i]));
            }
        }

        _logger.LogError("SignalR startup connection failed after all retries. Will connect on first notification.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_connection.State != HubConnectionState.Disconnected)
            {
                await _connection.StopAsync();
            }
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during SignalR connection disposal.");
        }

        _connectionLock.Dispose();
    }
}
