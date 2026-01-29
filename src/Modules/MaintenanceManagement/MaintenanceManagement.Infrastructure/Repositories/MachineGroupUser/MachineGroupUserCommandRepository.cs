
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.MachineGroupUser
{
    public class MachineGroupUserCommandRepository : IMachineGroupUserCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public MachineGroupUserCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MachineGroupUser machineGroupUser)
        {
             var entry =_applicationDbContext.Entry(machineGroupUser);
            await _applicationDbContext.MachineGroupUser.AddAsync(machineGroupUser);
            await _applicationDbContext.SaveChangesAsync();

            return machineGroupUser.Id;
        }

        public async Task<bool> DeleteAsync(int id,MaintenanceManagement.Domain.Entities.MachineGroupUser machineGroupUser)
        {
             var machineGroupToDelete = await _applicationDbContext.MachineGroupUser.FirstOrDefaultAsync(u => u.Id == id);
            if (machineGroupToDelete != null)
            {
                machineGroupToDelete.IsDeleted = machineGroupUser.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(MaintenanceManagement.Domain.Entities.MachineGroupUser machineGroupUser)
        {
            var existingMachineGroupUser = await _applicationDbContext.MachineGroupUser
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == machineGroupUser.Id);
            
            if (existingMachineGroupUser != null)
            {
                existingMachineGroupUser.DepartmentId = machineGroupUser.DepartmentId;
                existingMachineGroupUser.UserId = machineGroupUser.UserId;
                existingMachineGroupUser.MachineGroupId = machineGroupUser.MachineGroupId;
                existingMachineGroupUser.IsActive = machineGroupUser.IsActive;
                _applicationDbContext.MachineGroupUser.Update(existingMachineGroupUser);

                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            
            return false; 
        }
    }
}