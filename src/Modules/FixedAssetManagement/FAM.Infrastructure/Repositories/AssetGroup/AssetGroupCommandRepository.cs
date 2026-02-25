using FAM.Application.Common.Interfaces.IAssetGroup;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetGroup
{
    public class AssetGroupCommandRepository : IAssetGroupCommandRepository
    {
         private readonly ApplicationDbContext _applicationDbContext;
         
        public AssetGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _applicationDbContext.AssetGroup.AnyAsync(c => c.Code == code);
        }
        
        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetGroup assetGroup)
        {
         // Auto-generate SortOrder
        assetGroup.SortOrder = await GetMaxSortOrderAsync() + 1;
        // Add the AssetGroup to the DbContext
        await _applicationDbContext.AssetGroup.AddAsync(assetGroup);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetGroup
        return assetGroup.Id;
        }
        public async Task<int> UpdateAsync(int id, FAM.Domain.Entities.AssetGroup assetGroup)
        {   
    
        var existingassetGroup = await _applicationDbContext.AssetGroup.FirstOrDefaultAsync(u => u.Id == id);

        // If the assetGroup does not exist
        if (existingassetGroup is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetGroup.GroupName = assetGroup.GroupName;
        existingassetGroup.SortOrder = assetGroup.SortOrder;
        existingassetGroup.IsActive = assetGroup.IsActive;
        existingassetGroup.GroupPercentage = assetGroup.GroupPercentage;

        // Mark the entity as modified
            _applicationDbContext.AssetGroup.Update(existingassetGroup);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.AssetGroup.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }
       
       public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId,decimal groupPercentage)
       {
        var isNameDuplicate = await _applicationDbContext.AssetGroup
            .AnyAsync(ag => ag.GroupName == name && ag.Id != excludeId && ag.GroupPercentage == groupPercentage);

        var isSortOrderDuplicate = await _applicationDbContext.AssetGroup
            .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

        return (isNameDuplicate, isSortOrderDuplicate);
       }

        public async Task<int> DeleteAsync(int Id, FAM.Domain.Entities.AssetGroup assetGroup)
        {
            // Fetch the assetGroup to delete from the database
            var assetGroupToDelete = await _applicationDbContext.AssetGroup.FirstOrDefaultAsync(u => u.Id == Id);

            // If the assetGroup does not exist
            if (assetGroupToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            assetGroupToDelete.IsDeleted = assetGroup.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success

            }
        }
    }
