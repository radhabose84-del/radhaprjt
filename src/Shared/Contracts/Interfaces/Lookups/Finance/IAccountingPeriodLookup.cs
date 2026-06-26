using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    /// <summary>
    /// US-GL03-01..05 (post-refactor 2026-06-26) — cross-module read of Finance.AccountingPeriod.
    /// Name ends with "Lookup" so the global CachedLookupDecorator caches results automatically
    /// (Shared.Infrastructure AddLookupCaching).
    /// </summary>
    public interface IAccountingPeriodLookup
    {
        Task<IReadOnlyList<AccountingPeriodLookupDto>> GetAllPeriodsForCompanyAsync(int companyId, CancellationToken ct = default);

        /// <summary>Returns the (single) AccountingPeriod row whose date range contains <paramref name="date"/>
        /// and whose CompanyId matches. Used by the backdate-enforcement service to discover which
        /// period a backdated voucher would post into.</summary>
        Task<AccountingPeriodLookupDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct = default);

        Task<AccountingPeriodLookupDto?> GetByIdAsync(int periodId, int companyId, CancellationToken ct = default);
    }
}
