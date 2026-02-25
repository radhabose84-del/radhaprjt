using Contracts.Dtos.Lookups.Warehouse;

namespace Contracts.Interfaces.Lookups.Warehouse
{
    public interface IBinLookup
    {
        Task<List<BinLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<BinLookupDto>> GetByIdsAsync(IEnumerable<int> binIds, CancellationToken ct = default);
        Task<List<BinLookupDto>> GetByWarehouseIdAsync(int warehouseId, CancellationToken ct = default);
        Task<List<BinLookupDto>> GetByRackIdAsync(int rackId, CancellationToken ct = default);
    }
}
