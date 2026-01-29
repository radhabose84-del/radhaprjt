
using FAM.Domain.Common;
using FAM.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace FAM.Application.Common.Interfaces.IDepreciationGroup
{
    public interface IDepreciationGroupCommandRepository
    {
        Task<DepreciationGroups> CreateAsync(DepreciationGroups depreciationGroup);
        Task<bool>  UpdateAsync(DepreciationGroups depreciationGroup);
        Task<int>  DeleteAsync(int depGroupId,DepreciationGroups depreciationGroup); 
        Task<bool> ExistsByAssetGroupIdAsync(int assetGroupId); 
        Task<bool> ExistsByCodeAsync(string code );
        Task<int> GetMaxSortOrderAsync();        
        Task<DepreciationGroups> CheckForDuplicatesAsync(int groupId, int depMethodId,int bookTypeId,int excludeId);        
    }
}