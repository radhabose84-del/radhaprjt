using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceCategory
{
    public interface IMaintenanceCategoryCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory);
        Task<int> UpdateAsync(int Id,MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory);
        Task<int> DeleteAsync(int Id,MaintenanceManagement.Domain.Entities.MaintenanceCategory maintenanceCategory);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId);
        Task<bool> ExistsByCodeAsync(string? CategoryName);
    }
}