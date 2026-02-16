using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IItemPurchaseToleranceLookup
    {
        Task<IReadOnlyList<ItemPurchaseToleranceLookupDto>> GetByIdsAsync(IEnumerable<int> itemIds, CancellationToken ct = default);
    }
}
