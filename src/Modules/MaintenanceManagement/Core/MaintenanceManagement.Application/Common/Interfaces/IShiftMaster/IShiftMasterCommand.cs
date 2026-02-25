using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.IShiftMaster
{
    public interface IShiftMasterCommand
    {
        Task<int> CreateAsync(ShiftMaster shiftMaster);     
        Task<bool> UpdateAsync(ShiftMaster shiftMaster);
        Task<bool> DeleteAsync(int id,ShiftMaster shiftMaster); 
    }
}