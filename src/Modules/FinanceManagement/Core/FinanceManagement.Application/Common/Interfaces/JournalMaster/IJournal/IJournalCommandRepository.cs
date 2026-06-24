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
            int journalId, int postedStatusId, string? financialYearName, string? postedByName, int postedById, DateTimeOffset postedAt, CancellationToken ct);

        // US-GL01-06B — apply an approval-workflow result to a DRAFT journal: Approved → status APPROVED
        // (stamps ApprovedBy/At); Rejected → status DRAFT (stamps RejectedBy/At). Returns false if missing.
        Task<bool> SetApprovalResultAsync(int id, int statusId, bool approved, string? actorName, DateTimeOffset at, CancellationToken ct);

        // US-GL01 reversal — atomically inserts + posts the mirror voucher and flips the original to REVERSED
        // (sanctioned transition the immutability trigger allows), all in one transaction.
        Task<PostJournalResultDto?> ReverseAsync(
            FinanceManagement.Domain.Entities.JournalHeader reversal, int originalId, int postedStatusId,
            int reversedStatusId, string? financialYearName, string? postedByName, int postedById, DateTimeOffset postedAt, CancellationToken ct);
    }
}
