using UserManagement.Domain.Entities;
using UserManagement.Application.Common.Interfaces.IRoleEntitlement;
using System.Data;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.RoleEntitlements
{
    public class RoleEntitlementQueryRepository : IRoleEntitlementQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public RoleEntitlementQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(UserManagement.Domain.Entities.UserRole?, IList<RoleModule>, IList<RoleParent>, IList<RoleChild>, IList<RoleMenuPrivileges>)> GetByIdAsync(int roleEntitlementId)
        {
            const string query = @"
                SELECT Id FROM [AppSecurity].[UserRole] WHERE Id = @RoleEntitlementId

                SELECT Id, ModuleId FROM [AppSecurity].[RoleModule] WHERE RoleId = @RoleEntitlementId

                SELECT m.Id AS MenuId, m.MenuName, m.ModuleId, m.ParentId FROM [AppData].[Menus] m
                INNER JOIN [AppSecurity].[RoleParent] rp ON rp.MenuId = m.Id
                WHERE rp.RoleId = @RoleEntitlementId AND m.IsDeleted = 0

                SELECT m.Id AS MenuId, m.MenuName, m.ModuleId, m.ParentId FROM [AppData].[Menus] m
                INNER JOIN [AppSecurity].[RoleChild] rc ON rc.MenuId = m.Id
                WHERE rc.RoleId = @RoleEntitlementId AND m.IsDeleted = 0

                SELECT rmenu.Id, rmenu.RoleId, rmenu.MenuId, rmenu.CanView, rmenu.CanAdd, rmenu.CanUpdate, rmenu.CanDelete, rmenu.CanApprove, rmenu.CanExport FROM [AppData].[Menus] m
                INNER JOIN [AppSecurity].[RoleMenuPrivilege] rmenu ON rmenu.MenuId = m.Id
                WHERE rmenu.RoleId = @RoleEntitlementId AND m.IsDeleted = 0";

            using var multi = await _dbConnection.QueryMultipleAsync(query, new { RoleEntitlementId = roleEntitlementId });

            var role = await multi.ReadFirstOrDefaultAsync<UserRole>();
            var modules = (await multi.ReadAsync<UserManagement.Domain.Entities.RoleModule>()).ToList();
            var parentMenus = (await multi.ReadAsync<RoleParent>()).ToList();
            var childMenus = (await multi.ReadAsync<RoleChild>()).ToList();
            var roleMenuPrivileges = (await multi.ReadAsync<RoleMenuPrivileges>()).ToList();

            return (role, modules, parentMenus, childMenus, roleMenuPrivileges);
        }

        public async Task<UserRole> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            const string query = @"
                SELECT Id, RoleName, IsActive, CreatedBy, CreatedAt, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedAt, ModifiedByName, ModifiedIP
                FROM AppSecurity.UserRole
                WHERE RoleName = @RoleName";

            var userRole = await _dbConnection.QueryFirstOrDefaultAsync<UserRole>(query, new { RoleName = roleName });
            return userRole ?? new UserRole();
        }

        public async Task<List<Modules>> GetRolePrivileges(int userid, CancellationToken cancellationToken)
        {
            var moduleDictionary = new Dictionary<int, Modules>();
            var menuDictionary = new Dictionary<int, UserManagement.Domain.Entities.Menu>();

            const string sql = @"
                SELECT * INTO #MENUPERMISSION FROM (
                    SELECT * FROM [AppSecurity].[RoleParent]
                    UNION
                    SELECT * FROM [AppSecurity].[RoleChild]
                ) X

                SELECT
                    M.Id, M.ModuleName,
                    Menu.Id, Menu.MenuName, Menu.MenuUrl, Menu.Type, Menu.ParentId,
                    RMP.Id, RMP.MenuId, RMP.CanAdd, RMP.CanView, RMP.CanApprove, RMP.CanDelete, RMP.CanExport, RMP.CanUpdate
                FROM [AppData].[Modules] M
                INNER JOIN [AppSecurity].[RoleModule] RM ON M.Id = RM.ModuleId
                INNER JOIN [AppSecurity].[UserRoleAllocation] URA ON URA.UserRoleId = RM.RoleId AND URA.IsActive = 1
                INNER JOIN [AppData].[Menus] Menu ON Menu.ModuleId = M.Id
                INNER JOIN #MENUPERMISSION PM ON PM.MenuId = Menu.Id AND PM.RoleId = URA.UserRoleId
                LEFT JOIN [AppSecurity].[RoleMenuPrivilege] RMP ON RMP.MenuId = Menu.Id AND RMP.RoleId = URA.UserRoleId
                WHERE URA.UserId = @UserId AND M.IsDeleted = 0 AND Menu.IsDeleted = 0
                ORDER BY Menu.ParentId, Menu.SortOrder;";

            var result = await _dbConnection.QueryAsync<Modules, UserManagement.Domain.Entities.Menu, RoleMenuPrivileges, Modules>(
                sql,
                (module, menu, privilege) =>
                {
                    if (!moduleDictionary.TryGetValue(module.Id, out var moduleEntry))
                    {
                        moduleEntry = module;
                        moduleEntry.Menus = new List<UserManagement.Domain.Entities.Menu>();
                        moduleDictionary.Add(module.Id, moduleEntry);
                    }

                    if (!menuDictionary.TryGetValue(menu.Id, out var menuEntry))
                    {
                        menuEntry = menu;
                        menuEntry.ChildMenus = new List<UserManagement.Domain.Entities.Menu>();
                        menuEntry.RoleMenus = new List<RoleMenuPrivileges>();
                        menuDictionary.Add(menu.Id, menuEntry);
                    }

                    if (privilege != null && !menuEntry.RoleMenus.Any(p => p.Id == privilege.Id))
                        menuEntry.RoleMenus.Add(privilege);

                    if (menu.ParentId == 0 || !menuDictionary.TryGetValue(menu.ParentId, out var parentMenu))
                    {
                        if (!moduleEntry.Menus.Any(m => m.Id == menuEntry.Id))
                            moduleEntry.Menus.Add(menuEntry);
                    }
                    else
                    {
                        if (!parentMenu.ChildMenus.Any(cm => cm.Id == menuEntry.Id))
                            parentMenu.ChildMenus.Add(menuEntry);
                    }

                    return moduleEntry;
                },
                new { UserId = userid },
                splitOn: "Id,Id"
            );

            return result.Distinct().ToList();
        }
    }
}
