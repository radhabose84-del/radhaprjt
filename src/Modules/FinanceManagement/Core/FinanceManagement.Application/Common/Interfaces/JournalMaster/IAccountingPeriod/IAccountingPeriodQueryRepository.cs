using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod
{
    public interface IAccountingPeriodQueryRepository
    {
        Task<(List<AccountingPeriodDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? financialYearId = null);
        Task<AccountingPeriodDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<AccountingPeriodLookupDto>> AutocompleteAsync(string term, int? companyId, int? financialYearId, CancellationToken ct);

        // Composite uniqueness: one period number per (company, fiscal year). Excludes self on update.
        Task<bool> AlreadyExistsAsync(int companyId, int financialYearId, int periodNo, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // FK validation: StatusId must be an active MiscMaster row of type PERIOD_STATUS.
        Task<bool> StatusExistsAsync(int statusId);
    }
}
