using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IUnitDetailLookup
    {
        Task<UnitDetailLookupDto?> GetByIdAsync(int unitId, CancellationToken ct = default);
    }
}
