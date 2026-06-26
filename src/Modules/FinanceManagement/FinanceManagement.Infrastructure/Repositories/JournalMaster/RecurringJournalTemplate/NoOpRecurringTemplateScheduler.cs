using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;

namespace FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate
{
    // Default scheduler used in hosts without Hangfire. The real Hangfire implementation in
    // BackgroundService.Infrastructure is registered after this one in the API/Worker hosts and wins.
    internal sealed class NoOpRecurringTemplateScheduler : IRecurringTemplateScheduler
    {
        public Task SyncAsync(int templateId, CancellationToken ct = default) => Task.CompletedTask;
        public void Remove(int templateId) { }
    }
}
