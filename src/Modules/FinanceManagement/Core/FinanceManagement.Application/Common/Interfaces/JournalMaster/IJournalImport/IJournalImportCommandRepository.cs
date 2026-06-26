namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport
{
    public interface IJournalImportCommandRepository
    {
        // Records the batch + row errors only (no journals) — used when validation fails (no partial commit).
        Task<int> SaveFailedBatchAsync(
            FinanceManagement.Domain.Entities.JournalImportBatch batch,
            IEnumerable<FinanceManagement.Domain.Entities.JournalImportError> errors,
            CancellationToken ct);

        // One transaction: save the batch then the draft journals (with ImportBatchId + an allocated VoucherNo).
        // financialYearNames maps FinancialYearId → name, used to format the voucher number. Returns batch + journal ids.
        Task<(int BatchId, List<int> JournalIds)> CommitAsync(
            FinanceManagement.Domain.Entities.JournalImportBatch batch,
            List<FinanceManagement.Domain.Entities.JournalHeader> drafts,
            IReadOnlyDictionary<int, string> financialYearNames,
            CancellationToken ct);
    }
}
