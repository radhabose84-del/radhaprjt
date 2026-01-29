using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Interfaces.IAssetGroup
{
    public interface IAssetGroupCommandRepository
    {
         Task<int> CreateAsync(FAM.Domain.Entities.AssetGroup assetGroup);
         Task<bool> ExistsByCodeAsync(string code );
         Task<int> GetMaxSortOrderAsync();
         Task<int> UpdateAsync(int Id,FAM.Domain.Entities.AssetGroup assetGroup);
         Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId,decimal groupPercentage);   
         Task<int> DeleteAsync(int Id,FAM.Domain.Entities.AssetGroup assetGroup);
   
    }
}