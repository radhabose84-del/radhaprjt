using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.IShiftMaster
{
    public interface IShiftMasterQuery
    {
        Task<(List<ShiftMaster>, int)> GetAllShiftMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<ShiftMaster> GetByIdAsync(int id);
        Task<List<ShiftMaster>> GetShiftMaster(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> AlreadyExistsAsync(string ShiftName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> AlreadyExistsShiftCodeAsync(string ShiftCode, int? id = null);
        Task<bool> IsShiftMasterLinkedAsync(int id);

    }
}