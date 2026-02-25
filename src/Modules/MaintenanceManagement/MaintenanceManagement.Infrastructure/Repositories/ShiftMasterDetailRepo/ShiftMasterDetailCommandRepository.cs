using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceManagement.Infrastructure.Repositories.ShiftMasterDetailRepo
{
    public class ShiftMasterDetailCommandRepository : IShiftMasterDetailCommand
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ShiftMasterDetailCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public async Task<int> CreateAsync(ShiftMasterDetail shiftMasterDetail)
        {
              var entry =_applicationDbContext.Entry(shiftMasterDetail);
            await _applicationDbContext.ShiftMasterDetail.AddAsync(shiftMasterDetail);
            await _applicationDbContext.SaveChangesAsync();

            return shiftMasterDetail.Id;
        }

        public async Task<bool> DeleteAsync(int id, ShiftMasterDetail shiftMasterDetail)
        {
             var shiftToDelete = await _applicationDbContext.ShiftMasterDetail.FirstOrDefaultAsync(u => u.Id == id);
            if (shiftToDelete != null)
            {
                shiftToDelete.IsDeleted = shiftMasterDetail.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(ShiftMasterDetail shiftMasterDetail)
        {
            var existingShiftMaster = await _applicationDbContext.ShiftMasterDetail
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == shiftMasterDetail.Id);
            
            if (existingShiftMaster != null)
            {
                existingShiftMaster.ShiftMasterId = shiftMasterDetail.ShiftMasterId;
                existingShiftMaster.UnitId = shiftMasterDetail.UnitId;
                existingShiftMaster.StartTime = shiftMasterDetail.StartTime;
                existingShiftMaster.EndTime = shiftMasterDetail.EndTime;
                existingShiftMaster.DurationInHours = shiftMasterDetail.DurationInHours;
                existingShiftMaster.BreakDurationInMinutes = shiftMasterDetail.BreakDurationInMinutes;
                existingShiftMaster.EffectiveDate = shiftMasterDetail.EffectiveDate;
                existingShiftMaster.ShiftSupervisorId = shiftMasterDetail.ShiftSupervisorId;
                existingShiftMaster.IsActive = shiftMasterDetail.IsActive;
                _applicationDbContext.ShiftMasterDetail.Update(existingShiftMaster);

                return await _applicationDbContext.SaveChangesAsync() >0;
            }
            
            return false; 
        }
    }
}