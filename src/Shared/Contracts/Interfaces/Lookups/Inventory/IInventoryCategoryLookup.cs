using Contracts.Dtos.Lookups.Inventory;

namespace Contracts.Interfaces.Lookups.Inventory
{
    public interface IInventoryCategoryLookup
    {
        Task<List<CategoryMasterDto>> GetCategoryByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    }
}
