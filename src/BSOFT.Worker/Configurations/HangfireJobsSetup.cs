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
            // CRITICAL: Hangfire jobs live in the Hangfire DB, NOT DefaultConnection (DB).
            var connectionString = (config.GetConnectionString("HangfireConnection") ?? string.Empty)
                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            using var connection = new SqlConnection(connectionString);
            connection.Open();

            // Step 1: Collect all zombie job IDs before deleting (needed for Set/State cleanup).
            const string collectSql = @"
                SELECT Id FROM [HangFire].[Job]
                WHERE InvocationData LIKE '%INotificationHandler%';
            ";
            var zombieIds = connection.Query<long>(collectSql).ToList();

            if (zombieIds.Count == 0)
            {
                logger.LogDebug("HangfireJobsSetup: No zombie INotificationHandler jobs found.");
                return;
            }

            // Step 2: Delete retry/schedule sets, state entries, and job parameters for these IDs.
            const string cleanupSql = @"
                -- Remove from retry set
                DELETE FROM [HangFire].[Set]
                WHERE [Key] LIKE 'retries:%'
                  AND [Value] IN (SELECT CAST(Id AS NVARCHAR(20)) FROM [HangFire].[Job] WHERE InvocationData LIKE '%INotificationHandler%');

                -- Remove from schedule set
                DELETE FROM [HangFire].[Set]
                WHERE [Key] LIKE 'schedule%'
                  AND [Value] IN (SELECT CAST(Id AS NVARCHAR(20)) FROM [HangFire].[Job] WHERE InvocationData LIKE '%INotificationHandler%');

                -- Remove state history
                DELETE FROM [HangFire].[State]
                WHERE JobId IN (SELECT Id FROM [HangFire].[Job] WHERE InvocationData LIKE '%INotificationHandler%');

                -- Remove job parameters
                DELETE FROM [HangFire].[JobParameter]
                WHERE JobId IN (SELECT Id FROM [HangFire].[Job] WHERE InvocationData LIKE '%INotificationHandler%');

                -- Remove from job queue
                DELETE FROM [HangFire].[JobQueue]
                WHERE JobId IN (SELECT Id FROM [HangFire].[Job] WHERE InvocationData LIKE '%INotificationHandler%');

                -- Finally delete the jobs themselves (ALL states: Scheduled, Enqueued, Failed, Processing, etc.)
                DELETE FROM [HangFire].[Job]
                WHERE InvocationData LIKE '%INotificationHandler%';
            ";

            var deleted = connection.Execute(cleanupSql);
            logger.LogInformation(
                "HangfireJobsSetup: Cleaned up {Count} zombie INotificationHandler jobs ({Ids}).",
                zombieIds.Count, string.Join(", ", zombieIds.Take(20)));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "HangfireJobsSetup: Failed to cleanup zombie jobs. Non-critical — they will be retried.");
        }
    }
}

