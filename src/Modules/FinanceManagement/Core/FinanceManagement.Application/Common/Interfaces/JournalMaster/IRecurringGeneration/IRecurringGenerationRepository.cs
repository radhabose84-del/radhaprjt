namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration
{
    public interface IRecurringGenerationRepository
    {
        // Active templates whose validity window covers the period date (with their lines).
        Task<IReadOnlyList<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader>> GetDueTemplatesAsync(DateOnly periodDate, CancellationToken ct);

        // Idempotency guard — has this template already been generated for this company + period?
        Task<bool> GenerationExistsAsync(int companyId, int templateId, string period, CancellationToken ct);

        // Atomically inserts the generated journal (+ lines) and its generation-log row in ONE transaction,
        // so a partial failure cannot leave an orphan draft that a retry would regenerate. Sets
        // log.GeneratedVoucherId and returns the new journal id.
        Task<int> CreateJournalWithLogAsync(
            FinanceManagement.Domain.Entities.JournalHeader header,
            FinanceManagement.Domain.Entities.RecurringGenerationLog log,
            CancellationToken ct);

        // Flags the log row as auto-posted after a successful post.
        Task MarkLogAutoPostedAsync(int logId, CancellationToken ct);
    }
}
