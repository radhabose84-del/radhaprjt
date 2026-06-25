using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IFinancialPeriodMasterLookup
    {
        Task<IReadOnlyList<FinancialPeriodMasterLookupDto>> GetAllPeriodsForCompanyAsync(int companyId, CancellationToken ct = default);
        Task<FinancialPeriodMasterLookupDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct = default);
        Task<FinancialPeriodMasterLookupDto?> GetByIdAsync(int periodId, int companyId, CancellationToken ct = default);
    }
}
