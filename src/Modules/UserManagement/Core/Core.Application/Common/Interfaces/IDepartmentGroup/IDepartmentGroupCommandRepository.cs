using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Application.Common.Interfaces.IDepartmentGroup
{
    public interface IDepartmentGroupCommandRepository
    {

        Task<int> CreateAsync(Core.Domain.Entities.DepartmentGroup departmentGroup);

        Task<bool> UpdateAsync(int id, Core.Domain.Entities.DepartmentGroup departmentGroup);

        Task<bool> DeleteAsync(int id, Core.Domain.Entities.DepartmentGroup departmentGroup);
       
        
    }
}