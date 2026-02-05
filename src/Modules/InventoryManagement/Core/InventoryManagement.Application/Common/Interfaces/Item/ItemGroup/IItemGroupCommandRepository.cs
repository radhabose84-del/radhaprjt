namespace InventoryManagement.Application.Common.Interfaces.Item.ItemGroup
{
    public interface IItemGroupCommandRepository
    {
        Task<int> CreateAsync(InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup);
        Task<int> UpdateAsync(int assetId, InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup);
        Task<int> DeleteAsync(int assetId, InventoryManagement.Domain.Entities.Item.ItemGroup itemGroup);
        Task<bool> ExistsByCodeAsync(string code);
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        Task<bool> IsNameDuplicateAsync(string name, int excludeId);
        Task<bool> IsCodeDuplicateAsync(string code,int excludeId);
    }
}