using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IItemGroupLookup
    {
        Task<List<ItemGroupLookupDto>> GetAllItemGroupsAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ItemGroupLookupDto>> GetItemGroupsByIdsAsync(IEnumerable<int> itemGroupIds, CancellationToken ct = default);
    }
}
