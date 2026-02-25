using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MachineGroup
{
    public class MachineGroupCommandRepository : IMachineGroupCommandRepository
    {

        private readonly ApplicationDbContext _dbContext;

         public MachineGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }

         public async   Task<MaintenanceManagement.Domain.Entities.MachineGroup> CreateAsync(MaintenanceManagement.Domain.Entities.MachineGroup machineGroup)
        {            
             await _dbContext.MachineGroup.AddAsync(machineGroup);
                await _dbContext.SaveChangesAsync();
                return machineGroup;
        }  

       

           public async Task<bool> UpdateAsync(int id,MaintenanceManagement.Domain.Entities.MachineGroup machineGroup)
          {
            var existingmachineGroup =await _dbContext.MachineGroup.FirstOrDefaultAsync(m =>m.Id == machineGroup.Id);
         
            if (existingmachineGroup != null)
            {
                existingmachineGroup.GroupName = machineGroup.GroupName;
                existingmachineGroup.Manufacturer = machineGroup.Manufacturer;
                existingmachineGroup.DepartmentId = machineGroup.DepartmentId;                                  
                existingmachineGroup.IsActive = machineGroup.IsActive;
                existingmachineGroup.PowerSource = machineGroup.PowerSource;
                _dbContext.MachineGroup.Update(existingmachineGroup);
                return await _dbContext.SaveChangesAsync() > 0;
            }
            return false;
         }

      

         public async Task<bool> DeleteAsync(int id, MaintenanceManagement.Domain.Entities.MachineGroup machineGroup)
        {
            var existingMachineGroup = await _dbContext.MachineGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (existingMachineGroup != null)
            {
                existingMachineGroup.IsDeleted = machineGroup.IsDeleted;
                return await _dbContext.SaveChangesAsync() >0;
            }
            return false; 
        }        






        
    }
}