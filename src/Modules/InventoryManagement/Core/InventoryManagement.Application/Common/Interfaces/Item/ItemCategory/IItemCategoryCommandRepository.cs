namespace InventoryManagement.Application.Common.Interfaces.Item.ItemCategory
{
    public interface IItemCategoryCommandRepository
    {
        Task<int> CreateAsync(
            InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory,
            List<int> moduleIds,
            List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig> unitConfigs);

        Task<int> UpdateAsync(
            int assetId,
            InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory,
            List<int> moduleIds,
            List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig> unitConfigs);

        Task<int> DeleteAsync(int assetId, InventoryManagement.Domain.Entities.Item.ItemCategory itemCategory);
        Task<bool> ExistsByNameAsync(string? name);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId);
    }
}
