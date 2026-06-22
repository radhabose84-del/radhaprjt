using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport
{
    public interface IJournalImportQueryRepository
    {
        Task<(List<JournalImportBatchDto>, int)> GetAllBatchesAsync(int pageNumber, int pageSize);
        Task<JournalImportBatchDto?> GetBatchByIdAsync(int id);   // includes error rows

        // MiscMaster id resolution.
        Task<int> GetStatusIdAsync(string code);          // JOURNAL_STATUS
        Task<int> GetSourceIdAsync(string code);          // JOURNAL_SOURCE
        Task<int> GetBatchStatusIdAsync(string code);     // IMPORT_BATCH_STATUS

        // Bulk FK validation (one query per dimension) for the import engine.
        Task<IReadOnlyCollection<int>> GetExistingGlAccountIdsAsync(IEnumerable<int> ids, int companyId);
        Task<IReadOnlyCollection<int>> GetExistingVoucherTypeIdsAsync(IEnumerable<int> ids, int companyId);
        Task<IReadOnlyCollection<int>> GetExistingCostCentreIdsAsync(IEnumerable<int> ids);
        Task<IReadOnlyCollection<int>> GetExistingProfitCentreIdsAsync(IEnumerable<int> ids);
        Task<IReadOnlyCollection<int>> GetExistingCurrencyIdsAsync(IEnumerable<int> ids);

        Task<(int PeriodId, int FinancialYearId)?> GetOpenPeriodByDateAsync(int companyId, DateOnly date);
    }
}
