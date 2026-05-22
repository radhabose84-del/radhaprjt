using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using Contracts.Interfaces;

namespace UserManagement.Infrastructure.Repositories.RoleEntitlements
{
    public class RoleEntitlementCommandRepository : IRoleEntitlementCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IPermissionService _permissionService;

        public RoleEntitlementCommandRepository(
            ApplicationDbContext applicationDbContext,
            IPermissionService permissionService)
        {
            _applicationDbContext = applicationDbContext;
            _permissionService    = permissionService;
        }

        public async Task<bool> SaveRoleEntitlementsAsync(int roleId, IList<RoleModule> roleModules, IList<RoleParent> roleParents, IList<RoleChild> roleChildren, IList<RoleMenuPrivileges> roleMenuPrivileges, CancellationToken cancellationToken)
        {
            _applicationDbContext.RoleModules.RemoveRange(_applicationDbContext.RoleModules.Where(x => x.RoleId == roleId));
            _applicationDbContext.RoleParent.RemoveRange(_applicationDbContext.RoleParent.Where(x => x.RoleId == roleId));
            _applicationDbContext.RoleChild.RemoveRange(_applicationDbContext.RoleChild.Where(x => x.RoleId == roleId));
            _applicationDbContext.RoleMenuPrivileges.RemoveRange(_applicationDbContext.RoleMenuPrivileges.Where(x => x.RoleId == roleId));
            await _applicationDbContext.SaveChangesAsync(cancellationToken);

            await _applicationDbContext.RoleModules.AddRangeAsync(roleModules, cancellationToken);
            await _applicationDbContext.RoleParent.AddRangeAsync(roleParents, cancellationToken);
            await _applicationDbContext.RoleChild.AddRangeAsync(roleChildren, cancellationToken);
            await _applicationDbContext.RoleMenuPrivileges.AddRangeAsync(roleMenuPrivileges, cancellationToken);

            var saved = await _applicationDbContext.SaveChangesAsync(cancellationToken) > 0;

            // Invalidate permission cache for all users currently assigned to this role
            // so their next request re-reads the freshly saved privileges from the database.
            if (saved)
            {
                var affectedUserIds = await _applicationDbContext.UserRoleAllocations
                    .Where(a => a.UserRoleId == roleId)
                    .Select(a => a.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var userId in affectedUserIds)
                    _permissionService.InvalidateCache(userId);
            }

            return saved;
        }

        public async Task<bool> ModuleExistsAsync(int moduleId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.Modules.AnyAsync(m => m.Id == moduleId, cancellationToken);
        }

        public async Task<bool> MenuExistsAsync(int menuId, CancellationToken cancellationToken)
        {
            return await _applicationDbContext.Menus.AnyAsync(m => m.Id == menuId, cancellationToken);
        }
    }
}
