using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    // Consumed cross-module by AR / AP / TX for tax-code resolution and computation (US-GL02-05A AC1).
    public interface ITaxCodeLookup
    {
        Task<IReadOnlyList<TaxCodeLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default);

        // Resolves the rate effective on the given date (never retroactive — AC3-A).
        Task<TaxCodeLookupDto?> GetByCodeEffectiveAsync(int companyId, string taxCode, DateOnly asOf, CancellationToken ct = default);
    }
}
