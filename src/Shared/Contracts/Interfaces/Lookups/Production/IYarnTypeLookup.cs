using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface IYarnTypeLookup
    {
        Task<IReadOnlyList<YarnTypeLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<YarnTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
