namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration
{
    // US-GL01-11B — invoked (by the Hangfire period-open job) to instantiate due recurring templates
    // for a period. Returns the number of journals generated. Idempotent per (template, period).
    public interface IRecurringJournalGenerationService
    {
        Task<int> GenerateForPeriodAsync(int companyId, int baseCurrencyId, string period, DateOnly voucherDate, CancellationToken ct);
    }
}
