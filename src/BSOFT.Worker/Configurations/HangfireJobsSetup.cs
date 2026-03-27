using Hangfire;

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
        //jobManager.RemoveIfExists("maintenance-outbox-processor");
        // Remove the old Hangfire recurring outbox job — replaced by
        // OutboxPollingHostedService (PeriodicTimer @ 15 s) for sub-minute polling.
        // Hangfire recurring jobs only support minute-level cron (5 fields).
        jobManager.RemoveIfExists("sql-outbox-processor");

        // NOTE: Do NOT remove "maintenance-outbox-processor" here — it is owned by BSOFT.Api
        // and runs on the "maintenance-jobs" queue for scheduling events
        // (MachineWiseScheduleCreationEvent, HeaderUpdateEvent, NextSchedulerCreatedEvent).

        return host;
    }
}

