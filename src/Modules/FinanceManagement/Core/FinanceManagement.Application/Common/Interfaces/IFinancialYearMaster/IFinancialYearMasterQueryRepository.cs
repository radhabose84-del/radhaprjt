using FinanceManagement.Application.FinancialYearMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster
{
    public interface IFinancialYearMasterQueryRepository
    {
        Task<(List<FinancialYearMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int companyId, int? statusId = null);

        Task<FinancialYearMasterDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<FinancialPeriodMasterDto>> GetPeriodsForCompanyAsync(int companyId, CancellationToken ct);

        Task<FinancialPeriodMasterDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct);

        Task<IReadOnlyList<FinancialYearMasterLookupDto>> AutocompleteAsync(string term, int companyId, CancellationToken ct);

        Task<FinancialYearMasterLookupDto?> GetLatestForCompanyAsync(int companyId, CancellationToken ct);

        Task<bool> AlreadyExistsByCodeAsync(string financialYearCode, int companyId, int? id = null);
        Task<bool> OverlapsExistingRangeAsync(DateOnly startDate, DateOnly endDate, int companyId, int? id = null);
        Task<bool> NotFoundAsync(int id);

        Task<bool> FinancialYearStatusExistsAsync(int statusId);
        Task<bool> FinancialPeriodStatusExistsAsync(int statusId);

        /// <summary>
        /// Resolves a MiscMaster row by (MiscTypeCode, ValueCode) — used by Create handler to get
        /// the 'OPEN' StatusId for FYS / FPS without hardcoding ids.
        /// </summary>
        Task<int> GetMiscMasterIdByCodeAsync(string miscTypeCode, string valueCode);

        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsFinancialYearLinkedAsync(int id);
    }
}
