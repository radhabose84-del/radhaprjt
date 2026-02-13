using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Workflow;

namespace Contracts.Interfaces.Lookups.Workflow
{
    public interface IWarehouseLookup
    {
        Task<List<WarehouseLookupDto>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<WarehouseLookupDto>> GetByIdsAsync(IEnumerable<int> warehouseIds, CancellationToken ct = default);
    }
}
