using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IDepartmentGroup
{
    public interface IDepartmentGroupQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.DepartmentGroup>, int)> GetAllDepartmentGroupAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<UserManagement.Domain.Entities.DepartmentGroup> GetDepartmentGroupByIdAsync(int id);

        Task<bool> SoftDeleteValidation(int Id);

        Task<List<UserManagement.Domain.Entities.DepartmentGroup>> GetAllDepartmentGroupAsync(string SearchDeptGroupName);


        Task<UserManagement.Domain.Entities.DepartmentGroup?> GetByDepartmentGroupNameAsync(string departmentGroupName);
        Task<bool> IsLinkedWithDepartmentsAsync(int departmentGroupId);
        
    }
}