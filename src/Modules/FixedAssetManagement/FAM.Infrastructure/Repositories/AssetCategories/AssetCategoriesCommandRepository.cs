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
            var normalizedName = (name ?? string.Empty).Trim().ToUpper();

            var isNameDuplicate = await _applicationDbContext.AssetCategories
            .AnyAsync(ag =>
                ag.Id != excludeId &&
                ag.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                ((ag.CategoryName ?? string.Empty).Trim().ToUpper() == normalizedName));

        var isSortOrderDuplicate = await _applicationDbContext.AssetCategories
            .AnyAsync(ag =>
                ag.SortOrder == sortOrder &&
                ag.Id != excludeId &&
                ag.IsDeleted == BaseEntity.IsDelete.NotDeleted);

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

        public async Task<bool> ExistsByNameAsync(string categoryName, int? excludeId)
        {
            categoryName = (categoryName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return false;
            }

            var normalizedName = categoryName.ToUpper();
            var query = _applicationDbContext.AssetCategories
                .Where(c => c.IsDeleted == BaseEntity.IsDelete.NotDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync(c => ((c.CategoryName ?? string.Empty).Trim().ToUpper()) == normalizedName);
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
