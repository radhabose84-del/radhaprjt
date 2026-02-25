using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IDepartment
{
    public interface IDepartmentCommandRepository
    {            
        Task<Department> CreateAsync(Department department);

         Task<int> UpdateAsync(int id, Department department);
        
         Task<int> DeleteAsync(int id,Department department);
              
         Task<bool> ExistsByCodeAsync(string Department);

          Task<bool> ExistsByNameupdateAsync(string deptname, int id);
   
    }
}