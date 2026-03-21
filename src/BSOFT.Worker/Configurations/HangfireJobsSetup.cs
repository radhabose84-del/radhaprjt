using BackgroundService.Infrastructure.Jobs;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace BSOFT.Worker.Configurations;

public static class HangfireJobsSetup
{
    /// <summary>
    /// Registers all recurring Hangfire jobs for the Worker service.
    /// Uses <see cref="IRecurringJobManager"/> (service-based API) instead of the
    /// static <c>RecurringJob</c> helper so that <c>JobStorage.Current</c> does not
    /// need to be set — required when hosting in a generic IHost (Worker Service).
    /// </summary>
    public static IHost RegisterRecurringJobs(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        // Remove stale orphaned job from previous architecture where MaintenanceManagement
        // had its own outbox processor. SqlOutboxProcessorJob (sql-outbox-processor) already
        // handles both purchase.OutboxMessages and maintenance.OutboxMessages.
        jobManager.RemoveIfExists("maintenance-outbox-processor");

        // Centralized SQL outbox processor — polls purchase.OutboxMessages and
        // maintenance.OutboxMessages every minute, publishes pending events to
        // RabbitMQ via MassTransit, and applies exponential-backoff retry logic.
        jobManager.AddOrUpdate<SqlOutboxProcessorJob>(
            recurringJobId: "sql-outbox-processor",
            queue:          "sql-outbox-queue",
            methodCall:     job => job.ProcessAsync(CancellationToken.None),
            cronExpression: Cron.Minutely());

        return host;
    }
}
