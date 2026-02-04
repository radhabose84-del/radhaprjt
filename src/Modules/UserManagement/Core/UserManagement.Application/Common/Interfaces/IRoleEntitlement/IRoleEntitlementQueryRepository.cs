using UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Application.Common.Interfaces.IRoleEntitlement
{
    public interface IRoleEntitlementQueryRepository
    {
        Task<(UserManagement.Domain.Entities.UserRole,IList<RoleModule>,IList<RoleParent> ,IList<RoleChild> ,IList<RoleMenuPrivileges>)> GetByIdAsync(int Id);
        Task<UserManagement.Domain.Entities.UserRole> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken);
        // Task<List<RoleEntitlement>> GetRoleEntitlementsByRoleNameAsync(string roleName, CancellationToken cancellationToken);                      
        Task<List<RoleEntitlement>> GetExistingRoleEntitlementsAsync(List<int> userRoleIds,  List<int> menuIds, CancellationToken cancellationToken);
        Task<List<UserManagement.Domain.Entities.Modules>> GetRolePrivileges(int userid, CancellationToken cancellationToken);
    }
}