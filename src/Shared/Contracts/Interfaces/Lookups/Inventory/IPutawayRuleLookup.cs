using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Inventory;
using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IPutawayRuleLookup
    {
        Task<IReadOnlyList<PutawayRuleLookupDto>> GetByIdsAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> warehouseIds,
            CancellationToken ct = default);

        Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsAsync(
            List<int> itemIds,
            List<int> warehouseIds,
            CancellationToken ct = default);

        Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsByWarehouseAsync(
            List<int> itemIds,
            List<int> warehouseIds,
            CancellationToken ct = default);
    }
}
