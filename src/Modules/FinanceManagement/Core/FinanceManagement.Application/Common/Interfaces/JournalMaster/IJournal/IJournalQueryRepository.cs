using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal
{
    public interface IJournalQueryRepository
    {
        Task<(List<JournalHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null);
        Task<JournalHeaderDto?> GetByIdAsync(int id);

        // Resolves the OPEN accounting period containing the date (returns period id + its fiscal year).
        Task<(int PeriodId, int FinancialYearId)?> GetOpenPeriodByDateAsync(int companyId, DateOnly date);

        // MiscMaster id resolution by type code + value code (e.g. JOURNAL_STATUS/DRAFT, JOURNAL_SOURCE/MANUAL).
        Task<int> GetStatusIdAsync(string code);
        Task<int> GetSourceIdAsync(string code);

        // FK validation helpers (same-module direct SQL).
        Task<bool> VoucherTypeExistsAsync(int voucherTypeId, int companyId);
        Task<bool> GlAccountExistsAsync(int glAccountId, int companyId);
        Task<IReadOnlyCollection<int>> GetCostCentreMandatoryAccountIdsAsync(IEnumerable<int> glAccountIds);
        Task<bool> CostCentreExistsAsync(int costCentreId);
        Task<bool> ProfitCentreExistsAsync(int profitCentreId);
        Task<bool> CurrencyExistsAsync(int currencyId);

        Task<bool> NotFoundAsync(int id);
        // True only while the journal is still a Draft (editable / deletable).
        Task<bool> IsDraftAsync(int id);

        // Posting guards (US-GL01-09).
        Task<bool> IsPostedAsync(int id);          // already Posted or Reversed
        Task<bool> IsBalancedAsync(int id);        // TotalDr == TotalCr and > 0
        Task<bool> IsPeriodOpenAsync(int id);      // the journal's accounting period is OPEN
    }
}
