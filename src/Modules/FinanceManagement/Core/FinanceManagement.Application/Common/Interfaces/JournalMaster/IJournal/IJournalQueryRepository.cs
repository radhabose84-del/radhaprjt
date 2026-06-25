using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal
{
    public interface IJournalQueryRepository
    {
        Task<(List<JournalHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? statusId = null);

        // Advanced filtered search (Journal List & Search screen) — flat grid rows (header + primary line).
        Task<(List<JournalListItemDto>, int)> SearchAsync(JournalSearchFilter filter, int pageNumber, int pageSize, int? companyId = null);

        // Vouchers eligible for posting (US-GL01-06B): APPROVED, or a system journal (source != MANUAL) still in DRAFT.
        Task<(List<JournalListItemDto>, int)> GetPostableAsync(int pageNumber, int pageSize, int? companyId = null);

        // Manual DRAFT vouchers awaiting approval (candidates for the approver's pending list, US-GL01-06B).
        // The caller further filters to the rows where the current user is the workflow approver.
        Task<(List<JournalListItemDto>, int)> GetPendingApprovalAsync(int pageNumber, int pageSize, int? companyId = null);

        // True when the voucher is a manual journal still in DRAFT (eligible to be submitted for approval).
        Task<bool> IsManualDraftAsync(int id);
        // The journal's UnitId (for the approval-workflow payload header). 0 when not found.
        Task<int> GetUnitIdAsync(int id);

        Task<JournalHeaderDto?> GetByIdAsync(int id);

        // Autocomplete (active, non-deleted) by VoucherNo / Narration, optionally filtered by JOURNAL_STATUS id.
        Task<IReadOnlyList<JournalLookupDto>> AutocompleteAsync(string term, int? companyId, int? statusId, CancellationToken ct);

        // Resolves the OPEN accounting period containing the date (returns period id + its fiscal year).
        Task<(int PeriodId, int FinancialYearId)?> GetOpenPeriodByDateAsync(int companyId, DateOnly date);

        // MiscMaster id resolution by type code + value code (e.g. JOURNAL_STATUS/DRAFT, JOURNAL_SOURCE/MANUAL).
        Task<int> GetStatusIdAsync(string code);
        Task<int> GetSourceIdAsync(string code);

        // FK validation helpers (same-module direct SQL).
        Task<bool> VoucherTypeExistsAsync(int voucherTypeId, int companyId);
        Task<bool> GlAccountExistsAsync(int glAccountId, int companyId);
        // US-GL02-10 (AC2) — of the given accounts, those flagged IsCompanyRestricted that belong to a
        // company OTHER than the posting company (they may not be posted to from another entity).
        Task<IReadOnlyCollection<int>> GetForeignRestrictedAccountIdsAsync(IEnumerable<int> glAccountIds, int companyId);
        Task<IReadOnlyCollection<int>> GetCostCentreMandatoryAccountIdsAsync(IEnumerable<int> glAccountIds);
        Task<bool> CostCentreExistsAsync(int costCentreId);
        Task<bool> ProfitCentreExistsAsync(int profitCentreId);
        Task<bool> CurrencyExistsAsync(int currencyId);

        // Duplicate-entry control: a non-deleted voucher with the same company, type, date, totals AND the
        // same set of (account, Dr, Cr) lines already exists (excluding excludeId on update). Warning-level —
        // the caller may override.
        Task<bool> IsPotentialDuplicateAsync(
            int companyId, int voucherTypeId, DateOnly voucherDate, decimal totalDr, decimal totalCr,
            IReadOnlyList<(int GlAccountId, decimal DrAmount, decimal CrAmount)> lines, int? excludeId);

        Task<bool> NotFoundAsync(int id);
        // True only while the journal is still a Draft (editable / deletable).
        Task<bool> IsDraftAsync(int id);

        // Posting guards (US-GL01-09).
        Task<bool> IsPostedAsync(int id);          // already Posted or Reversed
        Task<bool> IsBalancedAsync(int id);        // TotalDr == TotalCr and > 0
        Task<bool> IsPeriodOpenAsync(int id);      // the journal's accounting period is OPEN
        // Eligible to post: APPROVED, or a system (source != MANUAL) journal still in DRAFT. (US-GL01-06B/07)
        Task<bool> IsPostingEligibleAsync(int id);
        Task<bool> IsReversedAsync(int id);        // status is already REVERSED
        Task<bool> IsReversalAsync(int id);        // the voucher is itself a reversal (US-12 AC-4: cannot be reversed)
        Task<DateOnly?> GetPostingDateAsync(int id);                              // original's posting date (US-12 AC-3)
        Task<DateOnly?> GetNextOpenPeriodStartAsync(int companyId, DateOnly afterDate); // default reversal date (US-12 AC-3)
    }
}
