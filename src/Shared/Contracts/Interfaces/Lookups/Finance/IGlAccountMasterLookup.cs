using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IGlAccountMasterLookup
    {
        Task<IReadOnlyList<GlAccountMasterLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default);
        Task<GlAccountMasterLookupDto?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken ct = default);
        Task<GlAccountMasterLookupDto?> GetByCodeForCompanyAsync(string accountCode, int companyId, CancellationToken ct = default);
    }
}
