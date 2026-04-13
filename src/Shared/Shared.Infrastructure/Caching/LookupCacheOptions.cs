#nullable enable
namespace Shared.Infrastructure.Caching;

/// <summary>
/// Configuration options for lookup caching.
/// Controls cache duration, size limits, and expiration policies.
/// </summary>
public sealed class LookupCacheOptions
{
    /// <summary>
    /// Default cache duration for all lookup data. Default: 30 minutes.
    /// After this duration without access, the cached entry is removed.
    /// </summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Absolute expiration time for cache entries. Default: 24 hours.
    /// After this absolute time, cache entry is removed regardless of access.
    /// Set to null for no absolute expiration.
    /// </summary>
    public TimeSpan? AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum number of cached lookup entries. Default: 1000.
    /// When limit is reached, least recently used entries are evicted.
    /// </summary>
    public int SizeLimit { get; set; } = 1000;

    /// <summary>
    /// Enable detailed logging for cache operations (hits/misses). Default: false.
    /// Useful for debugging and monitoring cache effectiveness.
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = false;
}
