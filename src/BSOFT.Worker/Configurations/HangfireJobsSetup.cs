using Dapper;
using Hangfire;
using Microsoft.Data.SqlClient;

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

        // Remove the old Hangfire recurring outbox job — replaced by
        // OutboxPollingHostedService (PeriodicTimer @ 15 s) for sub-minute polling.
        // Hangfire recurring jobs only support minute-level cron (5 fields).
        jobManager.RemoveIfExists("sql-outbox-processor");

        // NOTE: Do NOT remove "maintenance-outbox-processor" here — it is owned by BSOFT.Api
        // and runs on the "maintenance-jobs" queue for scheduling events
        // (MachineWiseScheduleCreationEvent, HeaderUpdateEvent, NextSchedulerCreatedEvent).

        // Cleanup zombie INotificationHandler Hangfire jobs — these are from an old code version
        // that scheduled notifications via Hangfire. Now notifications use MassTransit/RabbitMQ.
        // The INotificationHandler<T> interface has NO implementations, so these jobs fail forever
        // and Hangfire keeps retrying them with a 4-minute backoff delay.
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        CleanupZombieNotificationJobs(config, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());

        return host;
    }

    private static void CleanupZombieNotificationJobs(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("HangfireJobsSetup");

        try
        {
            var connectionString = (config.GetConnectionString("DefaultConnection") ?? string.Empty)
                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // Delete all Scheduled/Enqueued/Retrying jobs that reference the dead INotificationHandler interface.
            // These jobs can never succeed — INotificationHandler<T> has no implementations.
            const string deleteSql = @"
                DELETE FROM [HangFire].[Job]
                WHERE InvocationData LIKE '%INotificationHandler%'
                  AND StateName IN ('Scheduled', 'Enqueued', 'Awaiting', 'Processing');

                DELETE FROM [HangFire].[Set]
                WHERE [Key] LIKE 'retries:%'
                  AND [Value] IN (
                    SELECT CAST(j.Id AS NVARCHAR(20))
                    FROM [HangFire].[Job] j
                    WHERE j.InvocationData LIKE '%INotificationHandler%'
                      AND j.StateName = 'Scheduled'
                  );
            ";

            var deleted = connection.Execute(deleteSql);
            if (deleted > 0)
                logger.LogInformation("HangfireJobsSetup: Cleaned up {Count} zombie INotificationHandler jobs.", deleted);
            else
                logger.LogDebug("HangfireJobsSetup: No zombie INotificationHandler jobs found.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "HangfireJobsSetup: Failed to cleanup zombie jobs. Non-critical — they will be retried.");
        }
    }
}

