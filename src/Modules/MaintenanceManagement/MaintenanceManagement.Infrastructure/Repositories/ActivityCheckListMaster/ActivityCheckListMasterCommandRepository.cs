#nullable disable
using MaintenanceManagement.Application.Common.Interfaces.IActivityCheckListMaster;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.ActivityCheckListMaster
{
    public class ActivityCheckListMasterCommandRepository : IActivityCheckListMasterCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;
        public ActivityCheckListMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async Task<MaintenanceManagement.Domain.Entities.ActivityCheckListMaster> CreateAsync(MaintenanceManagement.Domain.Entities.ActivityCheckListMaster activityCheckListMaster)
        {
            await _dbContext.ActivityCheckListMaster.AddAsync(activityCheckListMaster);
            await _dbContext.SaveChangesAsync();
            return activityCheckListMaster;
        }



        public async Task<bool> UpdateAsync(int id, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster activityCheckListMaster)
        {
            var existingActivityCheckListr = await _dbContext.ActivityCheckListMaster.FirstOrDefaultAsync(m => m.Id == activityCheckListMaster.Id);

            if (existingActivityCheckListr != null)
            {
                existingActivityCheckListr.ActivityId = activityCheckListMaster.ActivityId;
                existingActivityCheckListr.ActivityCheckList = activityCheckListMaster.ActivityCheckList;
                existingActivityCheckListr.UnitId   = activityCheckListMaster.UnitId;
                existingActivityCheckListr.IsActive = activityCheckListMaster.IsActive;

                _dbContext.ActivityCheckListMaster.Update(existingActivityCheckListr);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
        
        public async Task<bool> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.ActivityCheckListMaster  activityCheckListMaster)
        {
        
            var activityCheckListToDelete = await _dbContext.ActivityCheckListMaster.FirstOrDefaultAsync(u => u.Id == Id);
           
      
            activityCheckListToDelete.IsDeleted = activityCheckListMaster.IsDeleted;
            
           return await _dbContext.SaveChangesAsync() > 0;

            
        }


    }
}