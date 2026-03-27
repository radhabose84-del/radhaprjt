using BackgroundService.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace BSOFT.Worker.Services;

/// <summary>
/// Polls SQL outbox tables every 15 seconds using a <see cref="PeriodicTimer"/>.
/// Replaces the previous Hangfire recurring job (<c>sql-outbox-processor</c>) which
/// was limited to minute-level cron granularity, causing up to 60 s notification delay.
/// </summary>
internal sealed class OutboxPollingHostedService : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxPollingHostedService> _logger;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    public OutboxPollingHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxPollingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "OutboxPollingHostedService started — polling every {Seconds}s.",
            PollInterval.TotalSeconds);

        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<SqlOutboxProcessorJob>();
                await job.ProcessAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "OutboxPollingHostedService: Error during outbox processing. Will retry in {Seconds}s.",
                    PollInterval.TotalSeconds);
            }
        }
    }
}
