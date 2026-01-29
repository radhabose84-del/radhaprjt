using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IDepartmentGroup;
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.DepartmentGroup
{
    public class DepartmentGroupCommandRepository : IDepartmentGroupCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DepartmentGroupCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(Core.Domain.Entities.DepartmentGroup departmentGroup)
        {
            await _applicationDbContext.DepartmentGroup.AddAsync(departmentGroup);
            await _applicationDbContext.SaveChangesAsync();
            return departmentGroup.Id;
        }

        public async Task<bool> UpdateAsync(int id, Core.Domain.Entities.DepartmentGroup departmentGroup)
        {
            var existingDepartmentGroup = await _applicationDbContext.DepartmentGroup
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDepartmentGroup != null)
            {
                existingDepartmentGroup.DepartmentGroupCode = departmentGroup.DepartmentGroupCode;
                existingDepartmentGroup.DepartmentGroupName = departmentGroup.DepartmentGroupName;
                existingDepartmentGroup.IsActive = departmentGroup.IsActive;


                _applicationDbContext.DepartmentGroup.Update(existingDepartmentGroup);
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }
        

        public async Task<bool> DeleteAsync(int id, Core.Domain.Entities.DepartmentGroup departmentGroup)
        {
            var existingDepartmentGroup = await _applicationDbContext.DepartmentGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (existingDepartmentGroup != null)
            {
                existingDepartmentGroup.IsDeleted = departmentGroup.IsDeleted;
            
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }


       
    }
}