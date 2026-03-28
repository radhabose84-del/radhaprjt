using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface ICompanyDetailLookup
    {
        Task<CompanyDetailLookupDto?> GetByUnitIdAsync(int unitId, CancellationToken ct = default);
    }
}
