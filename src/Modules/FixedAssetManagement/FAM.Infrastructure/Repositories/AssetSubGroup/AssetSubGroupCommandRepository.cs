using FAM.Application.Common.Interfaces.IAssetSubGroup;
using FAM.Domain.Common;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetSubGroup
{
    public class AssetSubGroupCommandRepository : IAssetSubGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AssetSubGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _applicationDbContext.AssetSubGroup.AnyAsync(c => c.Code == code && c.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetSubGroup assetSubGroup)
        {
            // Auto-generate SortOrder
            assetSubGroup.SortOrder = await GetMaxSortOrderAsync() + 1;
            assetSubGroup.GroupId = assetSubGroup.GroupId; 
            // Add the AssetGroup to the DbContext
            await _applicationDbContext.AssetSubGroup.AddAsync(assetSubGroup);

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();

            // Return the ID of the created AssetGroup
            return assetSubGroup.Id;
        }
        public async Task<int> UpdateAsync(int id, FAM.Domain.Entities.AssetSubGroup assetSubGroup)
        {

            var existingAssetSubGroup = await _applicationDbContext.AssetSubGroup.FirstOrDefaultAsync(u => u.Id == id);

            // If the assetGroup does not exist
            if (existingAssetSubGroup is null)
            {
                return -1; //indicate failure
            }

            // Update the existing assetGroup properties
            existingAssetSubGroup.SubGroupName = assetSubGroup.SubGroupName;
            existingAssetSubGroup.SortOrder = assetSubGroup.SortOrder;
            existingAssetSubGroup.IsActive = assetSubGroup.IsActive;
            existingAssetSubGroup.GroupId = assetSubGroup.GroupId;

            // Mark the entity as modified
            _applicationDbContext.AssetSubGroup.Update(existingAssetSubGroup);

            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.AssetSubGroup.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }

        public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId)
        {
            var isNameDuplicate = await _applicationDbContext.AssetSubGroup
                .AnyAsync(ag => ag.SubGroupName == name && ag.Id != excludeId);

            var isSortOrderDuplicate = await _applicationDbContext.AssetSubGroup
                .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

            return (isNameDuplicate, isSortOrderDuplicate);
        }

        public async Task<int> DeleteAsync(int Id, FAM.Domain.Entities.AssetSubGroup assetSubGroup)
        {
            // Fetch the assetGroup to delete from the database
            var assetSubGroupToDelete = await _applicationDbContext.AssetSubGroup.FirstOrDefaultAsync(u => u.Id == Id);

            // If the assetGroup does not exist
            if (assetSubGroupToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            assetSubGroupToDelete.IsDeleted = assetSubGroup.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success

        }
        public async Task<bool> ExistsAsync(int groupId)
        {
            return await _applicationDbContext.AssetGroup.AnyAsync(x => x.Id == groupId && x.IsDeleted == BaseEntity.IsDelete.NotDeleted);
        }
        
    }
}
