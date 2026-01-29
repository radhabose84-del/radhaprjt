using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IRoleEntitlement
{
    public interface IRoleEntitlementCommandRepository
    {        
        Task<bool> AddRoleEntitlementsAsync(int roleId,IList<RoleModule> roleModules,IList<RoleParent> roleParents,IList<RoleChild> roleChildren,IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken);     
        Task <bool> UpdateRoleEntitlementsAsync(int roleId, IList<RoleModule> roleModules,IList<RoleParent> roleParents,IList<RoleChild> roleChildren,IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken);
        Task<int>  DeleteAsync(int roleId,RoleEntitlement RoleEntitlement);
               // Add these new method definitions
        // Task<List<RoleEntitlement>> GetExistingRoleEntitlementsAsync(List<int> userRoleIds, List<int> moduleIds, List<int> menuIds, CancellationToken cancellationToken);
        Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken);
        Task<bool> MenuExistsAsync(int menuId, CancellationToken cancellationToken);
        // Task<Core.Domain.Entities.UserRole> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
        // Task<List<RoleEntitlement>> GetRoleEntitlementsByRoleNameAsync(string roleName, CancellationToken cancellationToken);   
    }
}