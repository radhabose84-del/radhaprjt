
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IRoleEntitlement;

namespace UserManagement.Infrastructure.Repositories.RoleEntitlements
{
    public class RoleEntitlementCommandRepository : IRoleEntitlementCommandRepository
    {
    private readonly ApplicationDbContext _applicationDbContext;
    public RoleEntitlementCommandRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

  
    public async Task<bool> AddRoleEntitlementsAsync(int roleId,IList<RoleModule> roleModules,IList<RoleParent> roleParents,IList<RoleChild> roleChildren,IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken)
    {
          _applicationDbContext.RoleModules.RemoveRange(_applicationDbContext.RoleModules.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleParent.RemoveRange(_applicationDbContext.RoleParent.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleChild.RemoveRange(_applicationDbContext.RoleChild.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleMenuPrivileges.RemoveRange(_applicationDbContext.RoleMenuPrivileges.Where(x => x.RoleId == roleId));

             await _applicationDbContext.SaveChangesAsync();
             
        await _applicationDbContext.RoleModules.AddRangeAsync(roleModules);
        await _applicationDbContext.RoleParent.AddRangeAsync(roleParents);
        await _applicationDbContext.RoleChild.AddRangeAsync(roleChildren);
        await _applicationDbContext.RoleMenuPrivileges.AddRangeAsync(roleMenuPrivileges);

       return await _applicationDbContext.SaveChangesAsync() >0;
                        
    }

    public async Task<bool> UpdateRoleEntitlementsAsync(int roleId, IList<RoleModule> roleModules,IList<RoleParent> roleParents,IList<RoleChild> roleChildren,IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken)
    {
    
             _applicationDbContext.RoleModules.RemoveRange(_applicationDbContext.RoleModules.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleParent.RemoveRange(_applicationDbContext.RoleParent.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleChild.RemoveRange(_applicationDbContext.RoleChild.Where(x => x.RoleId == roleId));
             _applicationDbContext.RoleMenuPrivileges.RemoveRange(_applicationDbContext.RoleMenuPrivileges.Where(x => x.RoleId == roleId));

             await _applicationDbContext.SaveChangesAsync();
        

                
             await _applicationDbContext.RoleModules.AddRangeAsync(roleModules);
             await _applicationDbContext.RoleParent.AddRangeAsync(roleParents);
             await _applicationDbContext.RoleChild.AddRangeAsync(roleChildren);
             await _applicationDbContext.RoleMenuPrivileges.AddRangeAsync(roleMenuPrivileges);

           return  await _applicationDbContext.SaveChangesAsync()>0;

    
    }
    public async Task<int> DeleteAsync(int id, RoleEntitlement roleEntitlement)
    {
        var RoleEntitlementToDelete = await _applicationDbContext.RoleEntitlements.FirstOrDefaultAsync(u => u.Id == id);
            if (RoleEntitlementToDelete != null)
            {
                RoleEntitlementToDelete.IsDeleted = roleEntitlement.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync();
            }
            return 0; // No user found
    }
        // New validation methods
    public async Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Modules.AnyAsync(m => m.Id == moduleId, cancellationToken);
    }

    public async Task<bool> MenuExistsAsync(int menuId, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.Menus.AnyAsync(m => m.Id == menuId, cancellationToken);
    }

//  public async Task<UserRole> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
//     {
//         return await _applicationDbContext.UserRole.FirstOrDefaultAsync(r => r.RoleName == roleName, cancellationToken) ?? new UserRole();
//     }

//     public async Task<List<RoleEntitlement>> GetRoleEntitlementsByRoleNameAsync(string roleName, CancellationToken cancellationToken)
//     {
//         return await _applicationDbContext.RoleEntitlements
//             .Where(re => re.UserRole.RoleName == roleName)
//             .Include(re => re.Module)
//             .Include(re => re.Menu)
//             .ToListAsync(cancellationToken);
//     }

//         public async Task<List<RoleEntitlement>> GetExistingRoleEntitlementsAsync(List<int> userRoleIds, List<int> moduleIds, List<int> menuIds, CancellationToken cancellationToken)
//         {
//             return await _applicationDbContext.RoleEntitlements
//             .Where(re => userRoleIds.Contains(re.UserRoleId) && moduleIds.Contains(re.ModuleId) && menuIds.Contains(re.MenuId))
//             .ToListAsync(cancellationToken);
//         }
    }
}