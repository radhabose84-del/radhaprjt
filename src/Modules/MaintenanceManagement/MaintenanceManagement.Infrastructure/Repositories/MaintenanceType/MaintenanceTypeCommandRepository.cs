using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MaintenanceType
{
    public class MaintenanceTypeCommandRepository :IMaintenanceTypeCommandRepository
    {
         private readonly ApplicationDbContext _applicationDbContext;

        public MaintenanceTypeCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType)
        {
             // Add the MaintenanceType to the DbContext
                await _applicationDbContext.MaintenanceType.AddAsync(maintenanceType);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created MaintenanceType
                return maintenanceType.Id;
        }

        public async Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType)
        {
            // Fetch the MaintenanceType to delete from the database
            var maintenancetypeToDelete = await _applicationDbContext.MaintenanceType.FirstOrDefaultAsync(u => u.Id == Id);

            // If the MaintenanceType does not exist
            if (maintenancetypeToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            maintenancetypeToDelete.IsDeleted = maintenanceType.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string? TypeName)
        {
            if (string.IsNullOrWhiteSpace(TypeName))
             return false; // Return false if null/empty
             return await _applicationDbContext.MaintenanceType.AnyAsync(c => c.TypeName == TypeName);
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId)
        {
             return await _applicationDbContext.MaintenanceType
                .AnyAsync(cc => cc.TypeName == name && cc.Id != excludeId);
        }

        public async Task<int> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.MaintenanceType maintenanceType)
        {
            var existingmaintenancetype = await _applicationDbContext.MaintenanceType.FirstOrDefaultAsync(u => u.Id == Id);

        // If the MaintenanceType does not exist
        if (existingmaintenancetype is null)
        {
            return -1; //indicate failure
        }

        // Update the existing MaintenanceType properties
        existingmaintenancetype.TypeName = maintenanceType.TypeName;
        existingmaintenancetype.IsActive=maintenanceType.IsActive;


        // Mark the entity as modified
        _applicationDbContext.MaintenanceType.Update(existingmaintenancetype);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
    }
}