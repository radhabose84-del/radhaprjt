using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IDepartment;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Infrastructure.Repositories.Departments
{
    public class DepartmentCommandRepository :IDepartmentCommandRepository
    { 
    private readonly ApplicationDbContext _applicationDbContext;

    public  DepartmentCommandRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext=applicationDbContext;
    }    

    public async Task<Department> CreateAsync(Department department)
    {
            await _applicationDbContext.Department.AddAsync(department);
            await _applicationDbContext.SaveChangesAsync();
            return department;
    }

     public async Task<int>UpdateAsync(int id, Department department)
    {
            var existingDept = await _applicationDbContext.Department.FirstOrDefaultAsync(u => u.Id == id);
            if (existingDept != null)
            {
                existingDept.ShortName = department.ShortName;
                existingDept.DeptName = department.DeptName;
                existingDept.CompanyId = department.CompanyId;
                existingDept.DepartmentGroupId= department.DepartmentGroupId;
                existingDept.IsActive = department.IsActive;                                

                _applicationDbContext.Department.Update(existingDept);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
    }

    public async Task<int> DeleteAsync(int id ,Department department )
    {
        
        
            var deptToDelete = await _applicationDbContext.Department.FirstOrDefaultAsync(u => u.Id == id);
            if (deptToDelete != null)
            {
                deptToDelete.IsDeleted = IsDelete.Deleted;
                _applicationDbContext.Department.Update(deptToDelete);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
    }

        public Task<bool> ExistsByCodeAsync(string Department)
        {
        
            return _applicationDbContext.Department.AnyAsync(c => c.DeptName == Department);
            
        }
            public async Task<bool> ExistsByNameupdateAsync(string name, int id)
                    {
                        return await _applicationDbContext.Department.AnyAsync(c => c.DeptName == name && c.Id != id);
                    }

        
    }
}