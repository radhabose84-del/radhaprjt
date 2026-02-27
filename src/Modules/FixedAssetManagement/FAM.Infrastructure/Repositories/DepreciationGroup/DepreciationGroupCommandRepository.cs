using FAM.Application.Common.Interfaces.IDepreciationGroup;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.DepreciationGroup
{
    public class DepreciationGroupCommandRepository : IDepreciationGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;        
        public DepreciationGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;            
        }
        public async Task<DepreciationGroups> CreateAsync(DepreciationGroups depreciationGroup)
        {
            depreciationGroup.SortOrder = await GetMaxSortOrderAsync() + 1;
            await _applicationDbContext.DepreciationGroups.AddAsync(depreciationGroup);
            await _applicationDbContext.SaveChangesAsync();
            return depreciationGroup;          
        }
        public async Task<int> DeleteAsync(int depGroupId, DepreciationGroups depreciationGroup)
        {
            var DepGroupToDelete = await _applicationDbContext.DepreciationGroups.FirstOrDefaultAsync(u => u.Id == depGroupId);
            if (DepGroupToDelete != null)
            {
                DepGroupToDelete.IsDeleted = depreciationGroup.IsDeleted;              
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0;
        }
        public async Task<bool> UpdateAsync( DepreciationGroups depreciationGroup)
        {
            var existingDepGroup = await _applicationDbContext.DepreciationGroups.FirstOrDefaultAsync(u => u.Id == depreciationGroup.Id);             
    
            if (existingDepGroup != null)
            {
                existingDepGroup.Code = depreciationGroup.Code;
                existingDepGroup.BookType = depreciationGroup.BookType;                
                existingDepGroup.DepreciationGroupName = depreciationGroup.DepreciationGroupName;
                existingDepGroup.IsActive = depreciationGroup.IsActive;
                existingDepGroup.AssetGroupId = depreciationGroup.AssetGroupId;
                existingDepGroup.UsefulLife = depreciationGroup.UsefulLife;
                existingDepGroup.DepreciationMethod = depreciationGroup.DepreciationMethod;
                existingDepGroup.ResidualValue = depreciationGroup.ResidualValue;
                _applicationDbContext.DepreciationGroups.Update(existingDepGroup);
                return await _applicationDbContext.SaveChangesAsync()>0;
            }
           return false; 
        }
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _applicationDbContext.DepreciationGroups.AnyAsync(c => c.Code == code && c.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }
        public async Task<bool> ExistsByAssetGroupIdAsync(int assetGroupId)
        {
            return await _applicationDbContext.AssetGroup.AnyAsync(ag => ag.Id == assetGroupId  && ag.IsDeleted==BaseEntity.IsDelete.NotDeleted  && ag.IsActive==BaseEntity.Status.Active);           
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.DepreciationGroups.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }       

       public async Task<DepreciationGroups> CheckForDuplicatesAsync(
        int groupId, int depMethodId, int bookTypeId,int excludeId)
        {
           return (await _applicationDbContext.DepreciationGroups
            .FirstOrDefaultAsync(ag =>
            ag.AssetGroupId == groupId &&
            ag.DepreciationMethod == depMethodId &&
            ag.BookType == bookTypeId &&
            ag.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
            ag.Id != excludeId
        ))!;
        }
    }
}