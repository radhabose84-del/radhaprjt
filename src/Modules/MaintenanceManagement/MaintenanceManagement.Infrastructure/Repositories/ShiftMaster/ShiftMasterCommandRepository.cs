using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.ShiftMaster
{
    public class ShiftMasterCommandRepository : IShiftMasterCommand
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ShiftMasterCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.ShiftMaster shiftMaster)
        {
             var entry =_applicationDbContext.Entry(shiftMaster);
            await _applicationDbContext.ShiftMaster.AddAsync(shiftMaster);
            await _applicationDbContext.SaveChangesAsync();

            return shiftMaster.Id;
        }

        public async Task<bool> DeleteAsync(int id, MaintenanceManagement.Domain.Entities.ShiftMaster shiftMaster)
        {
             var shiftToDelete = await _applicationDbContext.ShiftMaster.FirstOrDefaultAsync(u => u.Id == id);
            if (shiftToDelete != null)
            {
                shiftToDelete.IsDeleted = shiftMaster.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(MaintenanceManagement.Domain.Entities.ShiftMaster shiftMaster)
        {
            var existingShiftMaster = await _applicationDbContext.ShiftMaster
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == shiftMaster.Id);
            
            if (existingShiftMaster != null)
            {
                existingShiftMaster.ShiftCode = shiftMaster.ShiftCode;
                existingShiftMaster.ShiftName = shiftMaster.ShiftName;
                existingShiftMaster.EffectiveDate = shiftMaster.EffectiveDate;
                existingShiftMaster.IsActive = shiftMaster.IsActive;
                _applicationDbContext.ShiftMaster.Update(existingShiftMaster);

                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            
            return false; 
        }
    }
}