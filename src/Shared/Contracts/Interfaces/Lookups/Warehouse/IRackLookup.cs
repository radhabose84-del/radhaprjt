using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Warehouse;

namespace Contracts.Interfaces.Lookups.Warehouse
{
    public interface IRackLookup
    {
        Task<List<RackLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RackLookupDto>> GetByIdsAsync(IEnumerable<int> rackIds, CancellationToken ct = default);
        Task<List<RackLookupDto>> GetByWarehouseIdAsync(int warehouseId, CancellationToken ct = default);
    }
}
