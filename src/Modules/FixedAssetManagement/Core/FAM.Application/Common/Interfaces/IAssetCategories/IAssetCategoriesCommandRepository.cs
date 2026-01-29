using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.Common.Interfaces.IAssetCategories
{
    public interface IAssetCategoriesCommandRepository
    {
         Task<int> CreateAsync(FAM.Domain.Entities.AssetCategories assetCategories);
         Task<bool> ExistsByCodeAsync(string code );
         Task<bool> ExistsByNameAsync(string categoryName );
         Task<int> GetMaxSortOrderAsync();
         Task<int> UpdateAsync(int Id,FAM.Domain.Entities.AssetCategories assetCategories);
         Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId);   
         Task<int> DeleteAsync(int Id,FAM.Domain.Entities.AssetCategories assetCategories);
    }
}