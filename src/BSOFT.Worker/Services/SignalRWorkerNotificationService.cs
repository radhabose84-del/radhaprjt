#nullable disable
using Microsoft.AspNetCore.SignalR.Client;

namespace BSOFT.Worker.Services;

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

        _connection.Closed += (error) =>
        {
            _logger.LogWarning(error, "SignalR connection closed. Auto-reconnect will attempt.");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            _logger.LogInformation("SignalR reconnected. ConnectionId: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        // FIX #2: Pre-connect at startup so first notification is fast
        _ = ConnectWithRetryAsync();
    }

    public async Task SendAsync(string method, object payload)
    {
        // FIX #1: No try/catch — let exception propagate to WorkerInAppNotifier
        // so it knows this user's push failed and can continue with next user
        await EnsureConnectedAsync();
        await _connection.InvokeAsync("BroadcastFromWorker", method, payload);
    }

    private async Task EnsureConnectedAsync()
    {
        if (_connection.State == HubConnectionState.Connected)
            return;

        // FIX #3: Timeout the lock acquisition — don't wait forever
        if (!await _connectionLock.WaitAsync(TimeSpan.FromSeconds(15)))
            throw new TimeoutException("SignalR connection lock acquisition timed out.");

        try
        {
            if (_connection.State == HubConnectionState.Connected)
                return;

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                _logger.LogInformation("SignalR connection established to hub.");
                return;
            }
            // Connecting or Reconnecting — release lock and wait outside
        }
        finally
        {
            _connectionLock.Release();
        }

        // FIX #3: Wait for Connecting/Reconnecting WITHOUT holding the lock
        // so other SendAsync calls aren't blocked
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (_connection.State != HubConnectionState.Connected && sw.Elapsed < TimeSpan.FromSeconds(10))
        {
            await Task.Delay(200);
        }

        if (_connection.State != HubConnectionState.Connected)
            throw new InvalidOperationException(
                $"SignalR connection stuck in {_connection.State} state after 10s.");
    }

    /// <summary>
    /// FIX #2: Connects at startup with retry so first notification doesn't hit cold connection.
    /// Runs in background — does not block DI.
    /// </summary>
    private async Task ConnectWithRetryAsync()
    {
        int[] delays = { 1, 2, 5, 10, 30 };
        for (var i = 0; i < delays.Length; i++)
        {
            try
            {
                if (_disposed) return;
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
                    "SignalR startup attempt {Attempt}/{Max} failed: {Error}. Retry in {Delay}s.",
                    i + 1, delays.Length, ex.Message, delays[i]);
                await Task.Delay(TimeSpan.FromSeconds(delays[i]));
            }
        }
        _logger.LogError("SignalR startup failed after all retries. Will connect on first notification.");
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_connection.State != HubConnectionState.Disconnected)
                await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during SignalR connection disposal.");
        }
        _connectionLock.Dispose();
    }
}
