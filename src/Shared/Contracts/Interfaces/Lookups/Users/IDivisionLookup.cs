using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users;

public interface IDivisionLookup
{
    Task<IReadOnlyList<DivisionLookupDto>> GetAllDivisionAsync();
    Task<IReadOnlyList<DivisionLookupDto>> GetByIdsAsync(IEnumerable<int> ids);
    Task<List<DivisionUnitLookupDto>> GetUnitsByDivisionAsync(int companyId, int divisionId, CancellationToken ct = default);
}
