#nullable enable
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Caching;

/// <summary>
/// Singleton service that owns one CancellationTokenSource per cached lookup interface.
/// Cache entries created by CachedLookupDecorator&lt;T&gt; attach a CancellationChangeToken
/// from the corresponding CTS, so cancelling the source evicts ALL cached entries for that
/// lookup interface in O(1) inside IMemoryCache.
/// </summary>
public class LookupCacheInvalidator
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokenSources =
        new(StringComparer.Ordinal);

    private readonly ILogger<LookupCacheInvalidator> _logger;

    public LookupCacheInvalidator(ILogger<LookupCacheInvalidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns the current CancellationToken to be attached to a cache entry for the given
    /// lookup interface. The same token is returned for every entry of that interface until
    /// Evict is called, at which point a fresh token is issued for new entries.
    /// </summary>
    public virtual CancellationToken GetEvictionToken(string lookupInterfaceName)
    {
        var cts = _tokenSources.GetOrAdd(
            lookupInterfaceName,
            _ => new CancellationTokenSource());

        return cts.Token;
    }

    /// <summary>
    /// Evicts every cached entry tagged with the current CancellationToken for the given lookup.
    /// The next GetEvictionToken call for the same lookup will create and return a fresh token,
    /// so future cache entries are unaffected.
    /// Failures are logged and swallowed: eviction MUST NEVER break a successful command response.
    /// </summary>
    public virtual void Evict(string lookupInterfaceName)
    {
        if (string.IsNullOrEmpty(lookupInterfaceName))
            return;

        if (!_tokenSources.TryRemove(lookupInterfaceName, out var cts))
            return;

        try
        {
            cts.Cancel();
            cts.Dispose();
            _logger.LogDebug("Lookup cache evicted for {Lookup}", lookupInterfaceName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Lookup cache eviction failed for {Lookup}", lookupInterfaceName);
        }
    }
}
