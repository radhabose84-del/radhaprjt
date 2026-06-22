using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal sealed class RoleUserLookupRepository : IRoleUserLookup
    {
        private readonly IDbConnection _dbConnection;

        public RoleUserLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<bool> UserHasRoleAsync(int userId, int roleId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM AppSecurity.UserRoleAllocation ura
                    WHERE ura.UserId = @UserId
                      AND ura.UserRoleId = @RoleId
                      AND ura.IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { UserId = userId, RoleId = roleId }, cancellationToken: ct));
        }

        public async Task<IReadOnlyList<int>> GetUserIdsByRoleAsync(int roleId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT DISTINCT ura.UserId
                FROM AppSecurity.UserRoleAllocation ura
                JOIN AppSecurity.Users u ON u.UserId = ura.UserId AND u.IsDeleted = 0
                WHERE ura.UserRoleId = @RoleId
                  AND ura.IsActive = 1";

            var rows = await _dbConnection.QueryAsync<int>(
                new CommandDefinition(sql, new { RoleId = roleId }, cancellationToken: ct));
            return rows.ToList();
        }

        public async Task<IReadOnlyList<string>> GetEmailsByRoleAsync(int roleId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT DISTINCT u.EmailId
                FROM AppSecurity.UserRoleAllocation ura
                JOIN AppSecurity.Users u ON u.UserId = ura.UserId AND u.IsDeleted = 0
                WHERE ura.UserRoleId = @RoleId
                  AND ura.IsActive = 1
                  AND u.EmailId IS NOT NULL
                  AND LTRIM(RTRIM(u.EmailId)) <> ''";

            var rows = await _dbConnection.QueryAsync<string>(
                new CommandDefinition(sql, new { RoleId = roleId }, cancellationToken: ct));
            return rows.ToList();
        }
    }
}
