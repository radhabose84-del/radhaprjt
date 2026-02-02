using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.ICostCenter
{
    public interface ICostCenterQueryRepository
    {
        Task<MaintenanceManagement.Domain.Entities.CostCenter?> GetByIdAsync(int Id);
        // Task<(List<MaintenanceManagement.Domain.Entities.CostCenter>, int)> GetAllCostCenterGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<MaintenanceManagement.Domain.Entities.CostCenter>> GetCostCenterGroups(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> DepartmentSoftDeleteValidation(int Id);
        Task<bool> IsCostCenterLinkedAsync(int id); //IsActive And Delete Validation 
        Task<(List<CostCenterDto>, int)> GetAllCostCenterListGroupAsync(
         int PageNumber, int PageSize, string? SearchTerm);
    }
}