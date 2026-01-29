using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IDepartmentGroup
{
    public interface IDepartmentGroupQueryRepository
    {
        Task<(List<Core.Domain.Entities.DepartmentGroup>, int)> GetAllDepartmentGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Core.Domain.Entities.DepartmentGroup> GetDepartmentGroupByIdAsync(int id);

        Task<bool> SoftDeleteValidation(int Id);

        Task<List<Core.Domain.Entities.DepartmentGroup>> GetAllDepartmentGroupAsync(string SearchDeptGroupName);


        Task<Core.Domain.Entities.DepartmentGroup?> GetByDepartmentGroupNameAsync(string departmentGroupName);
        Task<bool> IsLinkedWithDepartmentsAsync(int departmentGroupId);
        
    }
}