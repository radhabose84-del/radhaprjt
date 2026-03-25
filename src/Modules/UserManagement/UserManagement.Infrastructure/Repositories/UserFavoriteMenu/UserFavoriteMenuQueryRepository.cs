using System.Data;
using Dapper;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Dto;

namespace UserManagement.Infrastructure.Repositories.UserFavoriteMenu
{
    internal sealed class UserFavoriteMenuQueryRepository : IUserFavoriteMenuQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserFavoriteMenuQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<UserFavoriteMenuDto>> GetByUserIdAsync(int userId)
        {
            const string sql = @"
                SELECT
                    ufm.MenuId,
                    m.MenuName,
                    m.MenuUrl,
                    m.MenuIcon,
                    m.ModuleId,
                    mod.ModuleName
                FROM AppData.UserFavoriteMenu ufm
                INNER JOIN AppData.Menus m ON ufm.MenuId = m.Id
                    AND m.IsDeleted = 0 AND m.IsActive = 1
                INNER JOIN AppData.Modules mod ON m.ModuleId = mod.Id
                    AND mod.IsDeleted = 0
                WHERE ufm.UserId = @UserId
                ORDER BY ufm.CreatedAt DESC;
            ";

            var result = await _dbConnection.QueryAsync<UserFavoriteMenuDto>(sql, new { UserId = userId });
            return result.ToList();
        }

        public async Task<bool> AlreadyFavoritedAsync(int userId, int menuId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.UserFavoriteMenu
                    WHERE UserId = @UserId AND MenuId = @MenuId
                ) THEN 1 ELSE 0 END;
            ";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UserId = userId, MenuId = menuId });
        }

        public async Task<bool> MenuExistsAndActiveAsync(int menuId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.Menus
                    WHERE Id = @MenuId AND IsActive = 1 AND IsDeleted = 0
                ) THEN 1 ELSE 0 END;
            ";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { MenuId = menuId });
        }

        public async Task<bool> FavoriteExistsAsync(int userId, int menuId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM AppData.UserFavoriteMenu
                    WHERE UserId = @UserId AND MenuId = @MenuId
                ) THEN 1 ELSE 0 END;
            ";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UserId = userId, MenuId = menuId });
        }
    }
}
