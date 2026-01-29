using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MachineMaster
{
    public class MachineMasterCommandRepository : IMachineMasterCommandRepository
    {
          private readonly ApplicationDbContext _applicationDbContext;
        public MachineMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MachineMaster machineMaster)
        {
            // Add the MachineMaster to the DbContext
                await _applicationDbContext.MachineMaster.AddAsync(machineMaster);

                // Save changes to the database
                await _applicationDbContext.SaveChangesAsync();

                // Return the ID of the created MachineMaster
                return machineMaster.Id;
        }

        public async Task<bool> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.MachineMaster machineMaster)
        {
            // Fetch the MachineMaster to delete from the database
            var machinemasterToDelete = await _applicationDbContext.MachineMaster.FirstOrDefaultAsync(u => u.Id == Id);

            // If the MachineMaster does not exist
            if (machinemasterToDelete is null)
            {
                return false; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            machinemasterToDelete.IsDeleted = machineMaster.IsDeleted;

            // Save changes to the database 
           return await _applicationDbContext.SaveChangesAsync() > 0;

        }

        public async Task<bool> ExistsByCodeAsync(string? MachineCode)
        {
             if (string.IsNullOrWhiteSpace(MachineCode))
             return false; // Return false if null/empty
             return await _applicationDbContext.MachineMaster.AnyAsync(c => c.MachineCode == MachineCode);
        }

        public async Task<bool> IsNameDuplicateAsync(string? name,int machineGroupId, int excludeId)
        {
             return await _applicationDbContext.MachineMaster
                .AnyAsync(cc =>
                    cc.MachineName == name &&
                    cc.MachineGroupId == machineGroupId &&
                    cc.Id != excludeId);
        }
        public async Task<bool> IsCodeDuplicateAsync(string? code, int unitId, int excludeId)
        {
            return await _applicationDbContext.MachineMaster
                .AnyAsync(cc => cc.MachineCode == code 
                            && cc.UnitId == unitId
                            && cc.Id != excludeId);
        }

        public async Task<bool> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.MachineMaster machineMaster)
        {
            var existingmachinemaster = await _applicationDbContext.MachineMaster.FirstOrDefaultAsync(u => u.Id == Id);

            // If the MaintenanceType does not exist
            if (existingmachinemaster is null)
            {
                return false; //indicate failure
            }

            // Update the existing MaintenanceType properties
            existingmachinemaster.MachineCode = machineMaster.MachineCode;
            existingmachinemaster.MachineName = machineMaster.MachineName;
            existingmachinemaster.MachineGroupId = machineMaster.MachineGroupId;
            existingmachinemaster.UnitId = machineMaster.UnitId;
            existingmachinemaster.ProductionCapacity = machineMaster.ProductionCapacity;
            existingmachinemaster.UomId = machineMaster.UomId;
            existingmachinemaster.ShiftMasterId = machineMaster.ShiftMasterId;
            existingmachinemaster.CostCenterId = machineMaster.CostCenterId;
            existingmachinemaster.WorkCenterId = machineMaster.WorkCenterId;
            existingmachinemaster.InstallationDate = machineMaster.InstallationDate;
            existingmachinemaster.AssetId = machineMaster.AssetId;
            existingmachinemaster.LineNo = machineMaster.LineNo;
            existingmachinemaster.IsActive = machineMaster.IsActive;
            existingmachinemaster.IsProductionMachine = machineMaster.IsProductionMachine;


            // Mark the entity as modified
            _applicationDbContext.MachineMaster.Update(existingmachinemaster);

            // Save changes to the database
          return  await _applicationDbContext.SaveChangesAsync() > 0;

        }
    }
}