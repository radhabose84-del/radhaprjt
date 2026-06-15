using Contracts.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Shared.Infrastructure.Services;

internal sealed class AccessPolicyService : IAccessPolicyService
{
    private readonly IAccessPolicyQueryRepository _repo;
    private readonly IIPAddressService            _ipService;
    private readonly IMemoryCache                 _cache;

    private static readonly MemoryCacheEntryOptions CacheOptions =
        new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSize(1);

    public AccessPolicyService(
        IAccessPolicyQueryRepository repo,
        IIPAddressService            ipService,
        IMemoryCache                 cache)
    {
        _repo      = repo;
        _ipService = ipService;
        _cache     = cache;
    }

    public async Task<IReadOnlyList<int>?> GetAllowedValueIdsAsync(
        string policyCode, CancellationToken ct = default)
    {
        var userId = _ipService.GetUserId();

        // Unauthenticated — do not restrict; let auth middleware handle it
        if (userId <= 0)
            return null;

        var cacheKey = $"apol:{userId}:{policyCode}";

        if (_cache.TryGetValue(cacheKey, out CacheEntry? entry))
            return entry!.AllowedIds;

        IReadOnlyList<int>? result;

        var bypass = await _repo.CheckBypassAsync(userId);
        if (bypass)
        {
            result = null;
        }
        else
        {
            var roleIds = await _repo.GetUserRoleIdsAsync(userId);

            // No roles: policy cannot apply — treat as unrestricted
            if (!roleIds.Any())
            {
                result = null;
            }
            else
            {
                result = await _repo.GetAllowedValueIdsAsync(policyCode, roleIds);
            }
        }

        _cache.Set(cacheKey, new CacheEntry(result), CacheOptions);
        return result;
    }

    // Wraps nullable list so IMemoryCache.TryGetValue distinguishes cache-miss from cached-null
    private sealed record CacheEntry(IReadOnlyList<int>? AllowedIds);
}
