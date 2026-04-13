using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface IRawMaterialTypeLookup
    {
        Task<IReadOnlyList<RawMaterialTypeLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RawMaterialTypeLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
