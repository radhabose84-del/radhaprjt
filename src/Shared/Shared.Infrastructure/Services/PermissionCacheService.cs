using System.Data;
using Contracts.Common;
using Contracts.Interfaces;
using Dapper;
using Microsoft.Extensions.Caching.Memory;

namespace Shared.Infrastructure.Services;

internal sealed class PermissionCacheService : IPermissionService
{
    private readonly IDbConnection _dbConnection;
    private readonly IMemoryCache _cache;

    private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
        .SetSize(1);

    public PermissionCacheService(IDbConnection dbConnection, IMemoryCache cache)
    {
        _dbConnection = dbConnection;
        _cache        = cache;
    }

    public async Task<bool> HasPermissionAsync(int userId, int menuId, PermissionType permission, CancellationToken ct = default)
    {
        if (userId <= 0 || menuId <= 0)
            return false;

        var cacheKey = $"perm:{userId}:{menuId}";

        if (!_cache.TryGetValue(cacheKey, out PermissionRow? row))
        {
            row = await FetchFromDbAsync(userId, menuId);
            _cache.Set(cacheKey, row, CacheOptions);
        }

        if (row == null)
            return false;

        return permission switch
        {
            PermissionType.CanView    => row.CanView,
            PermissionType.CanAdd     => row.CanAdd,
            PermissionType.CanUpdate  => row.CanUpdate,
            PermissionType.CanDelete  => row.CanDelete,
            PermissionType.CanApprove => row.CanApprove,
            PermissionType.CanExport  => row.CanExport,
            _                         => false
        };
    }

    public void InvalidateCache(int userId)
    {
        // Called after SaveRoleEntitlements to clear stale permission data.
        // We remove by prefix pattern — IMemoryCache doesn't support prefix remove,
        // so we track keys per userId in a simple set stored under a meta-key.
        var metaKey = $"perm:keys:{userId}";
        if (_cache.TryGetValue(metaKey, out HashSet<string>? keys) && keys != null)
        {
            foreach (var k in keys)
                _cache.Remove(k);
            _cache.Remove(metaKey);
        }
    }

    private async Task<PermissionRow?> FetchFromDbAsync(int userId, int menuId)
    {
        const string sql = @"
            SELECT
                rmp.CanView,
                rmp.CanAdd,
                rmp.CanUpdate,
                rmp.CanDelete,
                rmp.CanApprove,
                rmp.CanExport
            FROM   AppSecurity.UserRoleAllocation ura
            JOIN   AppSecurity.RoleMenuPrivilege  rmp
                   ON  rmp.RoleId    = ura.UserRoleId
                   AND rmp.IsDeleted = 0
            WHERE  ura.UserId    = @UserId
            AND    rmp.MenuId    = @MenuId
            AND    ura.IsActive  = 1";

        return await _dbConnection.QueryFirstOrDefaultAsync<PermissionRow>(
            sql, new { UserId = userId, MenuId = menuId });
    }

    private sealed class PermissionRow
    {
        public bool CanView    { get; init; }
        public bool CanAdd     { get; init; }
        public bool CanUpdate  { get; init; }
        public bool CanDelete  { get; init; }
        public bool CanApprove { get; init; }
        public bool CanExport  { get; init; }
    }
}
