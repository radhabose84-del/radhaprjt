using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IRoleEntitlement
{
    public interface IRoleEntitlementCommandRepository
    {
        Task<bool> SaveRoleEntitlementsAsync(int roleId, IList<RoleModule> roleModules, IList<RoleParent> roleParents, IList<RoleChild> roleChildren, IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken);
        Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken);
        Task<bool> MenuExistsAsync(int menuId, CancellationToken cancellationToken);
    }
}
