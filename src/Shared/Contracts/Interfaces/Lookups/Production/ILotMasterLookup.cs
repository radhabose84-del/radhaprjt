using Contracts.Dtos.Lookups.Production;

namespace Contracts.Interfaces.Lookups.Production
{
    public interface ILotMasterLookup
    {
        Task<IReadOnlyList<LotMasterLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<LotMasterLookupDto>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
