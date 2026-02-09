using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IItemLookup
    {
        Task<IReadOnlyList<ItemLookupDto>> GetByIdsAsync(IEnumerable<int> itemIds, CancellationToken ct = default);
    }
}
