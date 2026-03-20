using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface IPackTypeLookup
    {
        Task<IReadOnlyList<PackTypeLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PackTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
