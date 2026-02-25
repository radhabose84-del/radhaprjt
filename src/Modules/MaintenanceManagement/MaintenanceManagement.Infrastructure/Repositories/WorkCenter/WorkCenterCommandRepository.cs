using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.WorkCenter
{
    public class WorkCenterCommandRepository : IWorkCenterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public WorkCenterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.WorkCenter workCenter)
        {
            // Add the WorkCenter to the DbContext
                await _applicationDbContext.WorkCenter.AddAsync(workCenter);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created WorkCenter
                return workCenter.Id;
        }

        public async Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.WorkCenter workCenter)
        {
             // Fetch the workCenter to delete from the database
            var workCenterToDelete = await _applicationDbContext.WorkCenter.FirstOrDefaultAsync(u => u.Id == Id);

            // If the workCenter does not exist
            if (workCenterToDelete is null)
            {
                return -1; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            workCenterToDelete.IsDeleted = workCenter.IsDeleted;

            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();

            return 1; // Indicate success
        }

        public async Task<bool> ExistsByCodeAsync(string? workCenterCode)
        {
             if (string.IsNullOrWhiteSpace(workCenterCode))
             return false; // Return false if null/empty
             return await _applicationDbContext.WorkCenter.AnyAsync(c => c.WorkCenterCode == workCenterCode);
        }

        public async Task<bool> IsNameDuplicateAsync(string? name, int excludeId)
        {
            return await _applicationDbContext.WorkCenter
                .AnyAsync(cc => cc.WorkCenterName == name && cc.Id != excludeId);
        }

        public async Task<int> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.WorkCenter workCenter)
        {
             var existingworkcenter = await _applicationDbContext.WorkCenter.FirstOrDefaultAsync(u => u.Id == Id);

        // If the costcenter does not exist
        if (existingworkcenter is null)
        {
            return -1; //indicate failure
        }

        // Update the existing costcenter properties
        existingworkcenter.WorkCenterName = workCenter.WorkCenterName;
        existingworkcenter.UnitId = workCenter.UnitId;
        existingworkcenter.DepartmentId = workCenter.DepartmentId;
        existingworkcenter.IsActive=workCenter.IsActive;


        // Mark the entity as modified
        _applicationDbContext.WorkCenter.Update(existingworkcenter);

        // Save changes to the database
        await _applicationDbContext.SaveChangesAsync();

        return 1; // Indicate success
        }
        
     
    }
}