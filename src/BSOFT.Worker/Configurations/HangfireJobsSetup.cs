#nullable disable
using Hangfire;

namespace BSOFT.Worker.Configurations;

public static class HangfireJobsSetup
{
    /// <summary>
    /// Registers all recurring Hangfire jobs for the Worker service.
    /// Fire-and-forget / delayed jobs are enqueued by BSOFT.Api and processed here.
    /// </summary>
    public static IHost RegisterRecurringJobs(this IHost host)
    {
        // Add RecurringJob.AddOrUpdate(...) calls here as maintenance schedules
        // are defined. Example:
        //
        // RecurringJob.AddOrUpdate<IMaintenanceJobProcessor>(
        //     "daily-preventive-schedule",
        //     job => job.ProcessAsync(CancellationToken.None),
        //     Cron.Daily);

        return host;
    }
}
