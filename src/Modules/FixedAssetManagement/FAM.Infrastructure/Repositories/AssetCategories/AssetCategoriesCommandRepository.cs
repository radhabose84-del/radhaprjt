using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetCategories;
using FAM.Domain.Common;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetCategories
{
    public class AssetCategoriesCommandRepository : IAssetCategoriesCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AssetCategoriesCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId)
        {
            var isNameDuplicate = await _applicationDbContext.AssetCategories
            .AnyAsync(ag => ag.CategoryName == name && ag.Id != excludeId);

        var isSortOrderDuplicate = await _applicationDbContext.AssetCategories
            .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

        return (isNameDuplicate, isSortOrderDuplicate);
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetCategories assetCategories)
        {
             // Auto-generate SortOrder
        assetCategories.SortOrder = await GetMaxSortOrderAsync() + 1;
        // Add the Assetcategories to the DbContext
        await _applicationDbContext.AssetCategories.AddAsync(assetCategories);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetGroup
        return assetCategories.Id;
        }

        public async Task<int> DeleteAsync(int Id, FAM.Domain.Entities.AssetCategories assetCategories)
        {
            // Fetch the assetcategories to delete from the database
            var assetCategoriesToDelete = await _applicationDbContext.AssetCategories.FirstOrDefaultAsync(u => u.Id == Id);

            // If the assetcategories does not exist
            if (assetCategoriesToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            assetCategoriesToDelete.IsDeleted = assetCategories.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _applicationDbContext.AssetCategories.AnyAsync(c => c.Code == code);
        }

        public async Task<bool> ExistsByNameAsync(string categoryName)
        {
           return await _applicationDbContext.AssetCategories.AnyAsync(c => c.CategoryName == categoryName && c.IsDeleted == BaseEntity.IsDelete.NotDeleted && c.IsActive == BaseEntity.Status.Active);
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.AssetCategories.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }

        public async Task<int> UpdateAsync(int Id, FAM.Domain.Entities.AssetCategories assetCategories)
        {
        var existingassetcategories = await _applicationDbContext.AssetCategories.FirstOrDefaultAsync(u => u.Id == Id);

        // If the assetGroup does not exist
        if (existingassetcategories is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetcategories.CategoryName = assetCategories.CategoryName;
        existingassetcategories.Description = assetCategories.Description;
        existingassetcategories.AssetGroupId=assetCategories.AssetGroupId;
        existingassetcategories.SortOrder = assetCategories.SortOrder;
        existingassetcategories.IsActive = assetCategories.IsActive;

        // Mark the entity as modified
        _applicationDbContext.AssetCategories.Update(existingassetcategories);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}