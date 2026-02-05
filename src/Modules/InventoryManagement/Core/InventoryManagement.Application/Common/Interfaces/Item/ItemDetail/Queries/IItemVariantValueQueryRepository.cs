// InventoryManagement.Application/Common/Interfaces/Item/ItemDetail/Queries/IItemVariantValueQueryRepository.cs
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries
{
    public interface IItemVariantValueQueryRepository
    {
        Task<Dictionary<int, List<string>>> GetForItemGroupedAsync(int itemId, CancellationToken ct = default);
        Task<HashSet<string>> GetExistingChildComboKeysAsync(int templateItemId, CancellationToken ct = default);
        Task<Dictionary<string, int>> GetExistingChildCombosWithIdsAsync(int templateItemId, CancellationToken ct = default);        
        Task<List<VariantValueDto>> GetForItemAsync(int itemId, CancellationToken ct = default);             
    }    
}
