using System.Data;
using Contracts.Interfaces;
using Dapper;
using Microsoft.Extensions.Caching.Memory;

namespace Shared.Infrastructure.Services;

/// <summary>
/// Resolves the data access filtering context for the current authenticated user.
/// Registered as Scoped — one instance per HTTP request.
/// Uses IMemoryCache for cross-request caching of role/access data (5-min sliding expiration).
/// </summary>
internal sealed class DataAccessFilterService : IDataAccessFilter
{
    private readonly IIPAddressService _ipAddressService;
    private readonly IDbConnection _dbConnection;
    private readonly IMemoryCache _cache;
    private DataAccessContext? _cachedContext;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DataAccessFilterService(
        IIPAddressService ipAddressService,
        IDbConnection dbConnection,
        IMemoryCache cache)
    {
        _ipAddressService = ipAddressService;
        _dbConnection = dbConnection;
        _cache = cache;
    }

    public async Task<DataAccessContext> GetContextAsync(CancellationToken ct = default)
    {
        // Per-request memoization — avoid recomputing within the same HTTP request
        if (_cachedContext != null) return _cachedContext;

        var userId = _ipAddressService.GetUserId();
        if (userId <= 0)
        {
            _cachedContext = DataAccessContext.Unrestricted;
            return _cachedContext;
        }

        // Step 1: Check if any of the user's roles has BypassDataAccess = true
        var bypass = await CheckBypassAsync(userId);
        if (bypass)
        {
            _cachedContext = DataAccessContext.Unrestricted;
            return _cachedContext;
        }

        // Step 2: Get allowed ItemGroupIds (union across all user's roles)
        var itemGroupIds = await GetAllowedItemGroupIdsAsync(userId);

        // Step 3: Get PartyId from JWT claim
        var partyId = _ipAddressService.GetPartyId();

        // Step 4: Get allowed CustomerIds (agent-level filtering)
        var customerIds = partyId.HasValue
            ? await GetAllowedCustomerIdsAsync(partyId.Value)
            : (IReadOnlySet<int>)new HashSet<int>();

        _cachedContext = new DataAccessContext
        {
            BypassDataAccess = false,
            PartyId = partyId,
            AllowedItemGroupIds = itemGroupIds,
            AllowedCustomerIds = customerIds,
            AllowedAgentIds = new HashSet<int>() // Officer-level deferred
        };
        return _cachedContext;
    }

    private async Task<bool> CheckBypassAsync(int userId)
    {
        var cacheKey = $"DataAccess:Bypass:{userId}";
        if (_cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM AppSecurity.UserRoleAllocation ura
                INNER JOIN AppSecurity.UserRole ur ON ur.Id = ura.UserRoleId
                WHERE ura.UserId = @UserId AND ura.IsActive = 1
                  AND ur.IsDeleted = 0 AND ur.IsActive = 1
                  AND ur.BypassDataAccess = 1
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        var result = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { UserId = userId });

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration
        });

        return result;
    }

    private async Task<IReadOnlySet<int>> GetAllowedItemGroupIdsAsync(int userId)
    {
        var cacheKey = $"DataAccess:ItemGroups:{userId}";
        if (_cache.TryGetValue(cacheKey, out HashSet<int>? cached) && cached != null)
            return cached;

        const string sql = @"
            SELECT DISTINCT rigm.ItemGroupId
            FROM AppSecurity.UserRoleAllocation ura
            INNER JOIN UserManagement.RoleItemGroupMapping rigm
                ON rigm.RoleId = ura.UserRoleId
                AND rigm.IsDeleted = 0 AND rigm.IsActive = 1
            WHERE ura.UserId = @UserId AND ura.IsActive = 1";

        var ids = await _dbConnection.QueryAsync<int>(sql, new { UserId = userId });
        var result = new HashSet<int>(ids);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration
        });

        return result;
    }

    private async Task<IReadOnlySet<int>> GetAllowedCustomerIdsAsync(int partyId)
    {
        var cacheKey = $"DataAccess:Customers:{partyId}";
        if (_cache.TryGetValue(cacheKey, out HashSet<int>? cached) && cached != null)
            return cached;

        const string sql = @"
            SELECT DISTINCT acm.CustomerId
            FROM Sales.AgentCustomerMapping acm
            WHERE acm.AgentId = @PartyId
              AND acm.IsDeleted = 0 AND acm.IsActive = 1
              AND acm.EffectiveFrom <= GETDATE()
              AND (acm.EffectiveTo IS NULL OR acm.EffectiveTo >= GETDATE())";

        var ids = await _dbConnection.QueryAsync<int>(sql, new { PartyId = partyId });
        var result = new HashSet<int>(ids);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration
        });

        return result;
    }
}
