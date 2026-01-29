using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace MaintenanceManagement.Infrastructure.Repositories.ActivityMaster
{
    public class ActivityMasterCommandRepository :IActivityMasterCommandRepository
    {
         private readonly ApplicationDbContext _dbContext;       

          public ActivityMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

        public async   Task<MaintenanceManagement.Domain.Entities.ActivityMaster> CreateAsync(MaintenanceManagement.Domain.Entities.ActivityMaster  activityMaster)
        {            
             await _dbContext.ActivityMaster.AddAsync(activityMaster);
                await _dbContext.SaveChangesAsync();
                return activityMaster;
        }  
        public async Task<int> UpdateAsync(UpdateActivityMasterDto activityMaster)
            {
                 var existingRecord = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                            .FirstOrDefaultAsync(_dbContext.ActivityMaster, a => a.Id == activityMaster.ActivityId);

                if (existingRecord == null)
                {
                    return 0; // Return 0 if not found, since int is expected
                }

                // Update properties
                existingRecord.ActivityName = activityMaster.ActivityName;
                existingRecord.Description = activityMaster.Description;
                existingRecord.DepartmentId = activityMaster.DepartmentId;
                existingRecord.UnitId = activityMaster.UnitId;
                existingRecord.EstimatedDuration = activityMaster.EstimatedDuration;
                existingRecord.ActivityType = activityMaster.ActivityType;
                existingRecord.IsActive = activityMaster.IsActive;
                
                

                _dbContext.ActivityMaster.Update(existingRecord);

                // Remove existing detail records
                var existingDetails = _dbContext.ActivityMachineGroup
                    .Where(x => x.ActivityMasterId == activityMaster.ActivityId);
                _dbContext.ActivityMachineGroup.RemoveRange(existingDetails);

                // Add new detail records
                if (activityMaster.UpdateActivityMachineGroup != null && activityMaster.UpdateActivityMachineGroup.Any())
                {
                    var newDetails = activityMaster.UpdateActivityMachineGroup
                        .Select(x => new ActivityMachineGroup
                        {
                            ActivityMasterId = activityMaster.ActivityId,
                            MachineGroupId = x.MachineGroupId
                        }).ToList();

                    await _dbContext.ActivityMachineGroup.AddRangeAsync(newDetails);
                }

                var rowsAffected = await _dbContext.SaveChangesAsync();
                return rowsAffected; // Return number of affected rows
            }
   
       
    }
}