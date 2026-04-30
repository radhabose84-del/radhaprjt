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

        // Step 4: Get allowed CustomerIds and AgentIds
        IReadOnlySet<int> customerIds;
        var agentIdSet = new HashSet<int>();
        var isCustomerRestricted = false;

        if (partyId.HasValue)
        {
            // Agent-level: PartyId = AgentId → direct AgentCustomerMapping
            customerIds = await GetAllowedCustomerIdsAsync(partyId.Value);
            isCustomerRestricted = true;
            if (customerIds.Count > 0)
            {
                agentIdSet.Add(partyId.Value);
            }
            else
            {
                // No agent mappings — user is a customer, allow seeing their own party
                customerIds = new HashSet<int> { partyId.Value };
            }
        }
        else
        {
            // Step 4b: Marketing Officer — EmpId → OfficerAgent → AgentCustomerMapping
            var empId = _ipAddressService.GetEmpId();
            if (empId.HasValue && empId.Value > 0)
            {
                var officerAgentIds = await GetOfficerAgentIdsAsync(empId.Value);
                if (officerAgentIds.Count > 0)
                {
                    customerIds = await GetAllowedCustomerIdsByAgentsAsync(officerAgentIds);
                    foreach (var agentId in officerAgentIds)
                        agentIdSet.Add(agentId);
                }
                else
                {
                    customerIds = new HashSet<int>();
                }
                isCustomerRestricted = true;
            }
            else
            {
                customerIds = new HashSet<int>();
            }
        }

        _cachedContext = new DataAccessContext
        {
            BypassDataAccess = false,
            PartyId = partyId,
            AllowedItemGroupIds = itemGroupIds,
            AllowedCustomerIds = customerIds,
            AllowedAgentIds = agentIdSet,
            IsCustomerRestricted = isCustomerRestricted
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
            SlidingExpiration = CacheDuration,
            Size = 1
        });

        return result;
    }

    private async Task<IReadOnlySet<int>?> GetAllowedItemGroupIdsAsync(int userId)
    {
        // Step A: Check if RoleItemGroupMapping has ANY active rows globally.
        // If the table is empty, the feature is not configured — return null (no filtering).
        if (!await IsItemGroupMappingConfiguredAsync())
            return null;

        // Step B: Feature is configured — get this user's allowed IDs.
        var cacheKey = $"DataAccess:ItemGroups:{userId}";
        if (_cache.TryGetValue(cacheKey, out HashSet<int>? cached) && cached != null)
            return cached;

        const string sql = @"
            SELECT DISTINCT rigm.ItemGroupId
            FROM AppSecurity.UserRoleAllocation ura
            INNER JOIN AppSecurity.RoleItemGroupMapping rigm
                ON rigm.RoleId = ura.UserRoleId
                AND rigm.IsDeleted = 0 AND rigm.IsActive = 1
            WHERE ura.UserId = @UserId AND ura.IsActive = 1";

        var ids = await _dbConnection.QueryAsync<int>(sql, new { UserId = userId });
        var result = new HashSet<int>(ids);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = 1
        });

        return result;
    }

    /// <summary>
    /// Returns true if RoleItemGroupMapping has at least one active row globally.
    /// When false, item-group filtering is not configured — all users see all groups.
    /// </summary>
    private async Task<bool> IsItemGroupMappingConfiguredAsync()
    {
        const string cacheKey = "DataAccess:ItemGroupMappingConfigured";
        if (_cache.TryGetValue(cacheKey, out bool configured))
            return configured;

        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM AppSecurity.RoleItemGroupMapping
                WHERE IsDeleted = 0 AND IsActive = 1
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";

        var result = await _dbConnection.ExecuteScalarAsync<bool>(sql);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = 1
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
              AND acm.EffectiveFrom <= CAST(GETDATE() AS date)
              AND (acm.EffectiveTo IS NULL OR acm.EffectiveTo >= CAST(GETDATE() AS date))";

        var ids = await _dbConnection.QueryAsync<int>(sql, new { PartyId = partyId });
        var result = new HashSet<int>(ids);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = 1
        });

        return result;
    }

    private async Task<IReadOnlyList<int>> GetOfficerAgentIdsAsync(int empId)
    {
        var cacheKey = $"DataAccess:OfficerAgents:{empId}";
        if (_cache.TryGetValue(cacheKey, out List<int>? cached) && cached != null)
            return cached;

        // OfficerAgent has MarketingOfficerId (not EmpId) and no IsDeleted column (hard delete only)
        const string sql = @"
            SELECT DISTINCT oa.AgentId
            FROM Sales.OfficerAgent oa
            WHERE oa.MarketingOfficerId = @EmpId
              AND oa.IsActive = 1
              AND CAST(GETDATE() AS date) BETWEEN oa.ValidityFrom AND oa.ValidityTo";

        var ids = (await _dbConnection.QueryAsync<int>(sql, new { EmpId = empId })).ToList();

        _cache.Set(cacheKey, ids, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = 1
        });

        return ids;
    }

    private async Task<IReadOnlySet<int>> GetAllowedCustomerIdsByAgentsAsync(IReadOnlyList<int> agentIds)
    {
        var key = string.Join(",", agentIds.OrderBy(x => x));
        var cacheKey = $"DataAccess:OfficerCustomers:{key}";
        if (_cache.TryGetValue(cacheKey, out HashSet<int>? cached) && cached != null)
            return cached;

        const string sql = @"
            SELECT DISTINCT acm.CustomerId
            FROM Sales.AgentCustomerMapping acm
            WHERE acm.AgentId IN @AgentIds
              AND acm.IsDeleted = 0 AND acm.IsActive = 1
              AND acm.EffectiveFrom <= CAST(GETDATE() AS date)
              AND (acm.EffectiveTo IS NULL OR acm.EffectiveTo >= CAST(GETDATE() AS date))";

        var ids = await _dbConnection.QueryAsync<int>(sql, new { AgentIds = agentIds });
        var result = new HashSet<int>(ids);

        _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration,
            Size = 1
        });

        return result;
    }

}
