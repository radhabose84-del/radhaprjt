using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IWorkCenter
{
    public interface IWorkCenterCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.WorkCenter workCenter);
        Task<bool> ExistsByCodeAsync(string? WorkCenterCode);
        Task<int> UpdateAsync(int Id,MaintenanceManagement.Domain.Entities.WorkCenter workCenter);
        Task<int> DeleteAsync(int Id,MaintenanceManagement.Domain.Entities.WorkCenter workCenter);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId);
    }
}