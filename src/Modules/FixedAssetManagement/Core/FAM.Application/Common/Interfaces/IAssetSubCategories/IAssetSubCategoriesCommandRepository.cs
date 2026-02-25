namespace FAM.Application.Common.Interfaces.IAssetSubCategories
{
    public interface IAssetSubCategoriesCommandRepository
    {
         Task<int> CreateAsync(FAM.Domain.Entities.AssetSubCategories assetSubCategories);
         Task<bool> ExistsByCodeAsync(string code );
         Task<bool> ExistsByNameAsync(string subcategoryName);
         Task<int> GetMaxSortOrderAsync();
         Task<int> UpdateAsync(int Id,FAM.Domain.Entities.AssetSubCategories assetSubCategories);
         Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId);   
         Task<int> DeleteAsync(int Id,FAM.Domain.Entities.AssetSubCategories assetSubCategories);
    }
}