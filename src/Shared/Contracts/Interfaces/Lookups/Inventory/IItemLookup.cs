using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IItemLookup
    {
        Task<IReadOnlyList<ItemLookupDto>> GetByIdsAsync(IEnumerable<int> itemIds, CancellationToken ct = default);
        Task<IReadOnlyList<ItemLookupDto>> GetVariantsByParentIdAsync(int parentItemId, CancellationToken ct = default);
    }
}
