using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IAccountTypeMasterLookup
    {
        Task<IReadOnlyList<AccountTypeMasterLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default);
        Task<AccountTypeMasterLookupDto?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken ct = default);
    }
}
