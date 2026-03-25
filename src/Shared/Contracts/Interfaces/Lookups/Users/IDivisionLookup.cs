using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users;

public interface IDivisionLookup
{
    Task<IReadOnlyList<DivisionLookupDto>> GetAllDivisionAsync();
    Task<IReadOnlyList<DivisionLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
}
