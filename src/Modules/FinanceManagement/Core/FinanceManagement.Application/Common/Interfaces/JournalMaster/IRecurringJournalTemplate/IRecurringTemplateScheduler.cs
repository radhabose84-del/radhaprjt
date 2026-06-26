namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate
{
    // Creates/updates or removes the per-template auto-post Hangfire recurring job. Implemented in the
    // Hangfire host (BackgroundService.Infrastructure). SyncAsync re-evaluates the template's CURRENT
    // status + AutoPost and schedules a job ONLY for an Approved + AutoPost (active, not-ended) template;
    // otherwise it removes any existing job. The cron is derived from the template's Frequency anchored at StartDate.
    public interface IRecurringTemplateScheduler
    {
        Task SyncAsync(int templateId, CancellationToken ct = default);
        void Remove(int templateId);
    }
}
