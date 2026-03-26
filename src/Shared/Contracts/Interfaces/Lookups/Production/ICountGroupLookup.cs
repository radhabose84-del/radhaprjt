using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface ICountGroupLookup
    {
        Task<IReadOnlyList<CountGroupLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CountGroupLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
