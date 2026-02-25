using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MaintenanceCategory
{
    public class MaintenanceCategoryCommandRepository :IMaintenanceCategoryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public MaintenanceCategoryCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory)
        {
            // Add the MaintenanceCategory to the DbContext
                await _applicationDbContext.MaintenanceCategory.AddAsync(maintenanceCategory);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created MaintenanceCategory
                return maintenanceCategory.Id;
        }

        public async Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory)
        {
            // Fetch the MaintenanceCategory to delete from the database
            var maintenanceCategoryToDelete = await _applicationDbContext.MaintenanceCategory.FirstOrDefaultAsync(u => u.Id == Id);

            // If the MaintenanceCategory does not exist
            if (maintenanceCategoryToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            maintenanceCategoryToDelete.IsDeleted = maintenanceCategory.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string? CategoryName)
        {
             if (string.IsNullOrWhiteSpace(CategoryName))
             return false; // Return false if null/empty
             return await _applicationDbContext.MaintenanceCategory.AnyAsync(c => c.CategoryName == CategoryName);
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId)
        {
            return await _applicationDbContext.MaintenanceCategory
                .AnyAsync(cc => cc.CategoryName == name && cc.Id != excludeId);
        }

        public async Task<int> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory)
        {
              var existingmaintenanceCategory = await _applicationDbContext.MaintenanceCategory.FirstOrDefaultAsync(u => u.Id == Id);

        // If the costcenter does not exist
        if (existingmaintenanceCategory is null)
        {
            return -1; //indicate failure
        }

        // Update the existing costcenter properties
        existingmaintenanceCategory.CategoryName = maintenanceCategory.CategoryName;
        existingmaintenanceCategory.Description = maintenanceCategory.Description;
        existingmaintenanceCategory.IsActive=maintenanceCategory.IsActive;


        // Mark the entity as modified
        _applicationDbContext.MaintenanceCategory.Update(existingmaintenanceCategory);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}