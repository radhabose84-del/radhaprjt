using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface ICountMasterLookup
    {
        Task<IReadOnlyList<CountMasterLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CountMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
