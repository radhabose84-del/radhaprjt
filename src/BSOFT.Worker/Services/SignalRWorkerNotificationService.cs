#nullable disable
using Microsoft.AspNetCore.SignalR.Client;

namespace BSOFT.Worker.Services;

public sealed class SignalRWorkerNotificationService : IWorkerNotificationService, IAsyncDisposable
{
    private readonly ILogger<SignalRWorkerNotificationService> _logger;
    private readonly HubConnection _connection;

    public SignalRWorkerNotificationService(
        IConfiguration configuration,
        ILogger<SignalRWorkerNotificationService> logger)
    {
        _logger = logger;

        var hubUrl = configuration["SignalR:HubUrl"] ?? "https://localhost:7001/notificationHub";

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task SendAsync(string method, object payload)
    {
        try
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
                _logger.LogInformation("SignalR connection established to hub.");
            }

            await _connection.InvokeAsync("BroadcastFromWorker", method, payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification. Method: {Method}", method);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
