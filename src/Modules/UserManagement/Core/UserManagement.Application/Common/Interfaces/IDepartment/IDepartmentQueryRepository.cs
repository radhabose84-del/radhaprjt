using UserManagement.Application.Departments.Queries.GetDepartmentByGroupWithControl;
using UserManagement.Application.Departments.Queries.GetDepartments;
using UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IDepartment
{
    public interface IDepartmentQueryRepository
    {

        // Task<(IEnumerable<dynamic>, int)> GetAllDepartmentAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<(List<DepartmentDto>, int)> GetAllDepartmentAsync(int PageNumber, int PageSize, string? SearchTerm);

        Task<Department> GetByIdAsync(int id);
        Task<List<Department>> GetAllDepartmentAutoCompleteSearchAsync(string SearchDept);

        Task<bool> FKColumnExistValidation(int Id);
        Task<List<Department>> GetDepartment_SuperAdmin(string SearchDept);

        Task<List<DepartmentWithGroupDto>> GetDepartmentsByDepartmentGroupIdAsync(string departmentGroupName);
        Task<List<DepartmentWithControlByGroupDto>> GetDepartmentsByDepartmentGroupWithControl(string departmentGroupName);

        Task<bool> IsDepartmentUsedByAnyUserAsync(int departmentId);
        Task<bool> IsDepartmentLinkedAsync(int departmentId);


    
         
        
    }
}