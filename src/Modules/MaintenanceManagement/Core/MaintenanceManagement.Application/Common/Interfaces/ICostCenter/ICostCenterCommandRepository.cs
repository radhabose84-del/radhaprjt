using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.ICostCenter
{
    public interface ICostCenterCommandRepository
    {
        Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.CostCenter costCenter);
        Task<bool> ExistsByCodeAsync(string? costCenterCode);
        Task<int> UpdateAsync(int Id, MaintenanceManagement.Domain.Entities.CostCenter costCenter);
        Task<int> DeleteAsync(int Id, MaintenanceManagement.Domain.Entities.CostCenter costCenter);
        Task<bool> IsNameDuplicateAsync(string? name, int excludeId, int unitId);
        Task<bool> ExistsByCodeOrNameAndUnitAsync(string code, string name, int unitId);
      
    }
}