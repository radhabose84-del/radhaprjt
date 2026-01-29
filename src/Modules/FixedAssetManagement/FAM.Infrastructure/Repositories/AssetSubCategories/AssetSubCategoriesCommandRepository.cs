using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using FAM.Domain.Common;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetSubCategories
{
    public class AssetSubCategoriesCommandRepository : IAssetSubCategoriesCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public AssetSubCategoriesCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<(bool IsNameDuplicate, bool IsSortOrderDuplicate)> CheckForDuplicatesAsync(string name, int sortOrder, int excludeId)
        {
            var isNameDuplicate = await _applicationDbContext.AssetSubCategories
            .AnyAsync(ag => ag.SubCategoryName == name && ag.Id != excludeId);

        var isSortOrderDuplicate = await _applicationDbContext.AssetSubCategories
            .AnyAsync(ag => ag.SortOrder == sortOrder && ag.Id != excludeId);

        return (isNameDuplicate, isSortOrderDuplicate);
        }

        public async Task<int> CreateAsync(FAM.Domain.Entities.AssetSubCategories assetSubCategories)
        {     
        // Auto-generate SortOrder
        assetSubCategories.SortOrder = await GetMaxSortOrderAsync() + 1;
        // Add the AssetSubcategories to the DbContext
        await _applicationDbContext.AssetSubCategories.AddAsync(assetSubCategories);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        // Return the ID of the created AssetSubcategories
        return assetSubCategories.Id;
        }

        public async Task<int> DeleteAsync(int Id, FAM.Domain.Entities.AssetSubCategories assetSubCategories)
        {
            // Fetch the assetcategories to delete from the database
            var assetsubCategoriesToDelete = await _applicationDbContext.AssetSubCategories.FirstOrDefaultAsync(u => u.Id == Id);

            // If the assetcategories does not exist
            if (assetsubCategoriesToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            assetsubCategoriesToDelete.IsDeleted = assetSubCategories.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
           return await _applicationDbContext.AssetSubCategories.AnyAsync(c => c.Code == code);
        }

        public async Task<bool> ExistsByNameAsync(string subcategoryName)
        {
           return await _applicationDbContext.AssetSubCategories.AnyAsync(c => c.SubCategoryName == subcategoryName && c.IsDeleted == BaseEntity.IsDelete.NotDeleted && c.IsActive == BaseEntity.Status.Active);
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            return await _applicationDbContext.AssetSubCategories.MaxAsync(ac => (int?)ac.SortOrder) ?? -1;
        }

        public async Task<int> UpdateAsync(int Id, FAM.Domain.Entities.AssetSubCategories assetSubCategories)
        {
         var existingassetsubcategories = await _applicationDbContext.AssetSubCategories.FirstOrDefaultAsync(u => u.Id == Id);

        // If the assetGroup does not exist
        if (existingassetsubcategories is null)
        {
            return -1; //indicate failure
        }

        // Update the existing assetGroup properties
        existingassetsubcategories.SubCategoryName = assetSubCategories.SubCategoryName;
        existingassetsubcategories.Description = assetSubCategories.Description;
        existingassetsubcategories.AssetCategoriesId=assetSubCategories.AssetCategoriesId;
        existingassetsubcategories.SortOrder = assetSubCategories.SortOrder;
        existingassetsubcategories.IsActive = assetSubCategories.IsActive;

        // Mark the entity as modified
        _applicationDbContext.AssetSubCategories.Update(existingassetsubcategories);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}