#nullable enable
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Caching;

/// <summary>
/// Dynamic proxy that intercepts ALL method calls on lookup interfaces and adds caching.
/// Uses .NET's DispatchProxy for runtime interception - zero code changes needed in handlers/repositories.
/// </summary>
/// <typeparam name="TLookup">The lookup interface type (e.g., IUnitLookup, IUOMLookup)</typeparam>
public class CachedLookupDecorator<TLookup> : DispatchProxy where TLookup : class
{
    private TLookup _inner = null!;
    private IMemoryCache _cache = null!;
    private LookupCacheOptions _options = null!;
    private ILogger<CachedLookupDecorator<TLookup>> _logger = null!;

    /// <summary>
    /// Factory method to create a cached proxy for a lookup implementation.
    /// Called by DI container automatically.
    /// </summary>
    public static TLookup Create(
        TLookup inner,
        IMemoryCache cache,
        LookupCacheOptions options,
        ILogger<CachedLookupDecorator<TLookup>> logger)
    {
        var proxy = Create<TLookup, CachedLookupDecorator<TLookup>>() as CachedLookupDecorator<TLookup>;

        proxy!._inner = inner;
        proxy._cache = cache;
        proxy._options = options;
        proxy._logger = logger;

        return proxy as TLookup ?? throw new InvalidOperationException("Failed to create proxy");
    }

    /// <summary>
    /// Intercepts ALL method calls on the lookup interface and adds caching logic.
    /// </summary>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod == null)
            return null;

        // Generate unique cache key based on: InterfaceName_MethodName_SerializedArgs
        var cacheKey = GenerateCacheKey(targetMethod, args);

        // Check if this is an async method returning Task<T>
        var returnType = targetMethod.ReturnType;
        var isAsync = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);

        if (isAsync)
        {
            // Handle async methods
            return InvokeAsyncMethod(targetMethod, args, cacheKey);
        }
        else
        {
            // Handle sync methods (rare for lookups, but supported)
            return InvokeSyncMethod(targetMethod, args, cacheKey);
        }
    }

    private object? InvokeSyncMethod(MethodInfo method, object?[]? args, string cacheKey)
    {
        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out object? cached))
        {
            if (_options.EnableDetailedLogging)
                _logger.LogDebug("Cache HIT: {CacheKey} ({LookupType}.{Method})",
                    cacheKey, typeof(TLookup).Name, method.Name);

            return cached;
        }

        // Cache miss - invoke actual method
        if (_options.EnableDetailedLogging)
            _logger.LogDebug("Cache MISS: {CacheKey} ({LookupType}.{Method})",
                cacheKey, typeof(TLookup).Name, method.Name);

        var result = method.Invoke(_inner, args);

        // Cache the result
        CacheResult(cacheKey, result);

        return result;
    }

    private object InvokeAsyncMethod(MethodInfo method, object?[]? args, string cacheKey)
    {
        // Get the Task<T> result type
        var taskResultType = method.ReturnType.GetGenericArguments()[0];

        // Use reflection to call the generic InvokeAsyncMethodInternal<T>
        var internalMethod = typeof(CachedLookupDecorator<TLookup>)
            .GetMethod(nameof(InvokeAsyncMethodInternal), BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(taskResultType);

        return internalMethod.Invoke(this, new object?[] { method, args, cacheKey })!;
    }

    private async Task<TResult> InvokeAsyncMethodInternal<TResult>(
        MethodInfo method,
        object?[]? args,
        string cacheKey)
    {
        // Try to get from cache
        if (_cache.TryGetValue<TResult>(cacheKey, out var cached))
        {
            if (_options.EnableDetailedLogging)
                _logger.LogDebug("Cache HIT: {CacheKey} ({LookupType}.{Method})",
                    cacheKey, typeof(TLookup).Name, method.Name);

            return cached!;
        }

        // Cache miss - invoke actual method
        if (_options.EnableDetailedLogging)
            _logger.LogDebug("Cache MISS: {CacheKey} ({LookupType}.{Method})",
                cacheKey, typeof(TLookup).Name, method.Name);

        var task = method.Invoke(_inner, args) as Task<TResult>;
        var result = await task!;

        // Cache the result
        CacheResult(cacheKey, result);

        return result;
    }

    private void CacheResult(string cacheKey, object? result)
    {
        if (result == null)
            return; // Don't cache null results

        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = _options.CacheDuration,
            AbsoluteExpirationRelativeToNow = _options.AbsoluteExpiration,
            Size = 1 // For size-based eviction
        };

        _cache.Set(cacheKey, result, cacheOptions);

        if (_options.EnableDetailedLogging)
            _logger.LogDebug("Cached: {CacheKey} (expires in {Duration})",
                cacheKey, _options.CacheDuration);
    }

    private static string GenerateCacheKey(MethodInfo method, object?[]? args)
    {
        var lookupName = typeof(TLookup).FullName ?? typeof(TLookup).Name; // e.g., "Contracts.Interfaces.Lookups.UserManagement.IUnitLookup"
        var methodName = method.Name;          // e.g., "GetAllAsync"

        // Serialize arguments to create unique key
        var argsHash = args == null || args.Length == 0
            ? "NoArgs"
            : GetArgumentsHash(args);

        return $"{lookupName}:{methodName}:{argsHash}";
    }

    private static string GetArgumentsHash(object?[] args)
    {
        try
        {
            // Build a serializable representation that handles non-serializable types
            // like CancellationToken and correctly expands arrays/enumerables
            var parts = new List<string>();
            foreach (var arg in args)
            {
                if (arg == null)
                {
                    parts.Add("null");
                }
                else if (arg is CancellationToken)
                {
                    // CancellationToken is not serializable — skip it (it doesn't affect data)
                    continue;
                }
                else if (arg is System.Collections.IEnumerable enumerable and not string)
                {
                    // Expand arrays/lists so [155] and [200] produce different keys
                    var items = new List<string>();
                    foreach (var item in enumerable)
                        items.Add(item?.ToString() ?? "null");
                    parts.Add($"[{string.Join(",", items)}]");
                }
                else
                {
                    parts.Add(arg.ToString() ?? "null");
                }
            }

            var keyString = string.Join("_", parts);
            return Convert.ToBase64String(System.Security.Cryptography.MD5.HashData(
                System.Text.Encoding.UTF8.GetBytes(keyString)));
        }
        catch
        {
            // Last-resort fallback
            return string.Join("_", args.Select(a => a?.ToString() ?? "null"));
        }
    }
}
