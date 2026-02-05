namespace InventoryManagement.Application.Common.Interfaces.Item.ItemCategory
{
    public interface IItemCategoryCommandRepository
    {
        Task<int> CreateAsync(InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory);
        Task<int> UpdateAsync(int assetId, InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory);
        Task<int> DeleteAsync(int assetId, InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory);
        Task<bool> ExistsByNameAsync(string? name);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId);               
    }
}