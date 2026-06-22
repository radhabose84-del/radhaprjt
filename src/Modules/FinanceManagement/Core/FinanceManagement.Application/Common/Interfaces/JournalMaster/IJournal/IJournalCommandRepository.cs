using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal
{
    public interface IJournalCommandRepository
    {
        // Create header + lines in one save (draft — zero GL impact).
        Task<int> CreateAsync(FinanceManagement.Domain.Entities.JournalHeader entity);

        // Update a draft header + reconcile its lines (editable only while Draft).
        Task<int> UpdateAsync(FinanceManagement.Domain.Entities.JournalHeader entity);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // US-GL01-09 — atomic post: assign voucher number, update LedgerBalance, set status = Posted,
        // all in one DB transaction with rollback. Returns null if the journal is missing/already posted.
        Task<PostJournalResultDto?> PostAsync(
            int journalId, int postedStatusId, string? financialYearName, int postedBy, DateTimeOffset postedAt, CancellationToken ct);
    }
}
