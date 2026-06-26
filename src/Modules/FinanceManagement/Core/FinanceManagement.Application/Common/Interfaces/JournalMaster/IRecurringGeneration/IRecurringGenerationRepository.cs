namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration
{
    public interface IRecurringGenerationRepository
    {
        // One active template (with lines) by id — for forced per-template generate-now (ignores the date window).
        Task<FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader?> GetTemplateByIdAsync(int templateId, CancellationToken ct);

        // Idempotency guard — has this template already been generated for this company + period?
        Task<bool> GenerationExistsAsync(int companyId, int templateId, string period, CancellationToken ct);

        // Atomically inserts the generated journal (+ lines) and its generation-log row in ONE transaction,
        // so a partial failure cannot leave an orphan draft that a retry would regenerate. Allocates the voucher
        // number in the same transaction (when financialYearName is supplied and the header has none) so a failed
        // insert can't burn a number. Sets log.GeneratedVoucherId and returns the new journal id.
        Task<int> CreateJournalWithLogAsync(
            FinanceManagement.Domain.Entities.JournalHeader header,
            FinanceManagement.Domain.Entities.RecurringGenerationLog log,
            string? financialYearName,
            CancellationToken ct);

        // Flags the log row as auto-posted after a successful post.
        Task MarkLogAutoPostedAsync(int logId, CancellationToken ct);
    }
}
