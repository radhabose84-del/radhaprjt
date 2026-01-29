using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUserRole;

namespace UserManagement.Infrastructure.Repositories.UserRoles
{
    public class UserRoleCommandRepository :IUserRoleCommandRepository
    {
        
        private readonly ApplicationDbContext _applicationDbContext;

    public  UserRoleCommandRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext=applicationDbContext;
    } 
       public async Task<UserRole> CreateAsync(UserRole userrole)
    {
            await _applicationDbContext.UserRole.AddAsync(userrole);
            await _applicationDbContext.SaveChangesAsync();
            return userrole;
    }

        public async Task<int> DeleteAsync(int id ,UserRole userRole )
    {        
            var userroleToDelete = await _applicationDbContext.UserRole.FirstOrDefaultAsync(u => u.Id == id);
            if (userroleToDelete != null)            
            {                              
                userroleToDelete.IsDeleted = userRole.IsDeleted;
                 await _applicationDbContext.SaveChangesAsync();
                return  1;
            }
            return 0; // No user found
    }

     public async Task<int>UpdateAsync(int id, UserRole userrole)
    {
            var existingRole = await _applicationDbContext.UserRole.FirstOrDefaultAsync(u => u.Id == id);
            if (existingRole != null)
            {
                existingRole.RoleName = userrole.RoleName;
                existingRole.Description = userrole.Description;
                existingRole.CompanyId = userrole.CompanyId;
                existingRole.IsActive = userrole.IsActive;                
               
                _applicationDbContext.UserRole.Update(existingRole);
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
    }
    
    
     public Task<bool> ExistsByCodeAsync(string userrole)
        {
        
            return _applicationDbContext.UserRole.AnyAsync(c => c.RoleName == userrole);
            
        }
   public async Task<bool> ExistsByNameupdateAsync(string name, int id)
        {
            return await _applicationDbContext.UserRole.AnyAsync(c => c.RoleName == name && c.Id != id);
        }

    }
}