using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IDepartmentGroup
{
    public interface IDepartmentGroupCommandRepository
    {

        Task<int> CreateAsync(UserManagement.Domain.Entities.DepartmentGroup departmentGroup);

        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.DepartmentGroup departmentGroup);

        Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.DepartmentGroup departmentGroup);
       
        
    }
}