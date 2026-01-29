using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail
{
    public interface IShiftMasterDetailCommand
    {
        Task<int> CreateAsync(ShiftMasterDetail shiftMasterDetail);     
        Task<bool> UpdateAsync(ShiftMasterDetail shiftMasterDetail);
        Task<bool> DeleteAsync(int id,ShiftMasterDetail shiftMasterDetail); 
    }
}