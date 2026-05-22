using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackgroundService.Application.Notification.Common.Interfaces;
using Contracts.Dtos.Users;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Common
{
    public class LookupRepository : ILookupRepository
    {
        private const string DepartmentCacheKey = "LookupRepository_Departments";
        private const string UnitCacheKey = "LookupRepository_Units";
        private const string UserCacheKey = "LookupRepository_Users";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly IDbConnection _dbConnection;
        private readonly IMemoryCache _memoryCache;

        public LookupRepository(
            [FromKeyedServices("Notification")] IDbConnection dbConnection,
            IMemoryCache memoryCache)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<IReadOnlyDictionary<int, string>> GetDepartmentsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(DepartmentCacheKey, out IReadOnlyDictionary<int, string> cached))
            {
                return cached;
            }

            const string sql = @"
                SELECT DISTINCT Id DepartmentId, DeptName DepartmentName
                FROM AppData.Department
                WHERE Id IS NOT NULL and IsDeleted=0
                AND DeptName IS NOT NULL
                AND DeptName <> ''";

            var rows = await _dbConnection.QueryAsync<LookupRow>(new CommandDefinition(
                sql,
                cancellationToken: cancellationToken));

            var departmentLookup = rows
                .Where(row => row.DepartmentId.HasValue)
                .GroupBy(row => row.DepartmentId!.Value, row => row.DepartmentName!)
                .ToDictionary(g => g.Key, g => g.First());

            _memoryCache.Set(
                DepartmentCacheKey,
                departmentLookup,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration, Size = 1 });

            return departmentLookup;
        }

        public async Task<IReadOnlyDictionary<int, string>> GetUnitsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(UnitCacheKey, out IReadOnlyDictionary<int, string> cached))
            {
                return cached;
            }

            const string sql = @"
                SELECT DISTINCT Id UnitId, UnitName
                FROM AppData.Unit
                WHERE Id IS NOT NULL
                AND UnitName IS NOT NULL
                AND UnitName <> ''";

            var rows = await _dbConnection.QueryAsync<LookupRow>(new CommandDefinition(
                sql,
                cancellationToken: cancellationToken));

            var unitLookup = rows
                .Where(row => row.UnitId.HasValue)
                .GroupBy(row => row.UnitId!.Value, row => row.UnitName!)
                .ToDictionary(g => g.Key, g => g.First());

            _memoryCache.Set(
                UnitCacheKey,
                unitLookup,
                new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration, Size = 1 });

            return unitLookup;
        }

        public async Task<IReadOnlyDictionary<int, string>> GetMenuNamesAsync(
            IEnumerable<int> menuIds,
            CancellationToken cancellationToken = default)
        {
            var ids = menuIds?
                .Where(id => id != 0)
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
            {
                return new Dictionary<int, string>();
            }

            const string sql = @"
                SELECT Id MenuId, MenuName
                FROM AppData.Menus
                WHERE Id IN @Ids
                  AND IsDeleted = 0
                  AND MenuName IS NOT NULL
                  AND MenuName <> ''";

            var rows = await _dbConnection.QueryAsync<LookupRow>(new CommandDefinition(
                sql,
                new { Ids = ids },
                cancellationToken: cancellationToken));

            var menuLookup = rows
                .Where(row => row.MenuId.HasValue)
                .GroupBy(row => row.MenuId!.Value, row => row.MenuName!)
                .ToDictionary(g => g.Key, g => g.First());

            return menuLookup;
        }

        public async Task<IReadOnlyDictionary<int, string>> GetUserNamesAsync(
            IEnumerable<int> userIds,
            CancellationToken cancellationToken = default)
        {
            var ids = userIds?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? new List<int>();

            if (ids.Count == 0)
                return new Dictionary<int, string>();

            const string sql = @"
                SELECT UserId, UserName
                FROM AppSecurity.Users
                WHERE IsDeleted = 0
                  AND UserId IN @Ids";

            var rows = await _dbConnection.QueryAsync<LookupRow>(new CommandDefinition(
                sql,
                new { Ids = ids },
                cancellationToken: cancellationToken));

            return rows
                .Where(row => row.UserId.HasValue && !string.IsNullOrWhiteSpace(row.UserName))
                .GroupBy(row => row.UserId!.Value, row => row.UserName!)
                .ToDictionary(g => g.Key, g => g.First());
        }

        public async Task<int?> GetMenuIdByNameAsync(
            string menuName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(menuName))
                return null;

            const string sql = @"
                SELECT TOP 1 Id
                FROM AppData.Menus
                WHERE MenuName = @MenuName
                  AND IsDeleted = 0";

            var menuId = await _dbConnection.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
                sql,
                new { MenuName = menuName },
                cancellationToken: cancellationToken));

            return menuId;
        }

        public async Task<UserSessionDto?> GetSessionByJwtIdAsync(
            string jwtId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jwtId))
                return null;

            const string sql = @"
                SELECT
                    Id,
                    UserId,
                    JwtId,
                    BrowserInfo,
                    CreatedAt,
                    ExpiresAt,
                    IsActive,
                    LastActivity
                FROM AppSecurity.UserSessions
                WHERE JwtId = @JwtId";

            var session = await _dbConnection.QueryFirstOrDefaultAsync<UserSessionDto>(new CommandDefinition(
                sql,
                new { JwtId = jwtId },
                cancellationToken: cancellationToken));

            return session;
        }

        public async Task<bool> UpdateSessionLastActivityAsync(
            string jwtId,
            DateTimeOffset lastActivity,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(jwtId))
                return false;

            const string sql = @"
                UPDATE AppSecurity.UserSessions
                SET LastActivity = @LastActivity
                WHERE JwtId = @JwtId";

            var rowsAffected = await _dbConnection.ExecuteAsync(new CommandDefinition(
                sql,
                new { JwtId = jwtId, LastActivity = lastActivity },
                cancellationToken: cancellationToken));

            return rowsAffected > 0;
        }

        private sealed class LookupRow
        {
            public int? DepartmentId { get; init; }
            public string? DepartmentName { get; init; }
            public int? UnitId { get; init; }
            public string? UnitName { get; init; }
            public int? MenuId { get; init; }
            public string? MenuName { get; init; }
            public int? UserId { get; init; }
            public string? UserName { get; init; }
        }
    }
}
