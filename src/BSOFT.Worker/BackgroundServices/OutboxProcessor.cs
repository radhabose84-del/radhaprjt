#nullable disable
using Microsoft.Extensions.Hosting;

namespace BSOFT.Worker.BackgroundServices;

public class OutboxProcessor : Microsoft.Extensions.Hosting.BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                // TODO: Resolve outbox repository, poll OutboxMessages collection,
                //       publish each message to RabbitMQ via MassTransit IPublishEndpoint,
                //       then mark the message as processed in MongoDB.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxProcessor encountered an error.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped.");
    }
}
