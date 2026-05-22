#nullable enable
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Infrastructure.Caching;

/// <summary>
/// Extension methods for registering lookup caching across all modules.
/// Automatically discovers and decorates ALL I*Lookup interfaces with zero code changes.
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Registers global lookup caching for ALL I*Lookup interfaces across all modules.
    /// Uses DispatchProxy to intercept method calls and add caching layer transparently.
    ///
    /// ZERO CHANGES NEEDED in handlers or repositories - caching happens automatically!
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration for cache duration and limits</param>
    public static IServiceCollection AddLookupCaching(
        this IServiceCollection services,
        Action<LookupCacheOptions>? configure = null)
    {
        // 1️⃣ Configure options
        var options = new LookupCacheOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // 2️⃣ Register IMemoryCache (if not already registered)
        services.AddMemoryCache(memoryCacheOptions =>
        {
            memoryCacheOptions.SizeLimit = options.SizeLimit;
        });

        // 3️⃣ Register write-invalidate infrastructure:
        //    - LookupCacheInvalidator: singleton owning one CTS per cached lookup interface
        //    - CacheInvalidationBehavior: MediatR pipeline behavior that evicts after Create/Update/Delete commands
        services.AddSingleton<LookupCacheInvalidator>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheInvalidationBehavior<,>));

        // 4️⃣ Auto-discover ALL I*Lookup interfaces from Contracts assembly
        var lookupInterfaces = DiscoverLookupInterfaces();

        // 5️⃣ Decorate each lookup interface with caching proxy and record its name
        foreach (var lookupInterface in lookupInterfaces)
        {
            DecorateLookup(services, lookupInterface);
            RegisteredCachedLookupNames.Add(lookupInterface.Name);
        }

        Console.WriteLine($"✅ Lookup caching registered for {lookupInterfaces.Count} interfaces (Duration: {options.CacheDuration}, AbsoluteExp: {options.AbsoluteExpiration})");

        return services;
    }

    /// <summary>
    /// Discovers all interfaces ending with "Lookup" from the Contracts assembly.
    /// These are the lookup interfaces we want to cache (e.g., IUnitLookup, IUOMLookup, etc.)
    /// </summary>
    /// <summary>
    /// Lookup interfaces that must NOT be cached because they have write/mutation methods
    /// or return values that change frequently (e.g. document sequence counters).
    /// </summary>
    internal static readonly HashSet<string> ExcludedLookupInterfaces = new(StringComparer.Ordinal)
    {
        // IDocumentSequenceLookup has both write operations (IncrementDocNoAsync) and
        // frequently-changing reads (GenerateDocumentNumber changes after every doc created).
        // Caching either causes duplicate document numbers.
        "IDocumentSequenceLookup",

        // IWorkflowLookup queries AppData.ApprovalRequest which changes in real time
        // (new submissions, approvals, rejections). Caching causes pending approval lists
        // to show stale data — newly submitted items missing, already-approved items still appearing.
        "IWorkflowLookup",

        // ISupplierLookup scopes results to the caller's current unit (resolved from
        // IIPAddressService, not a method argument). The cache key is interface+method+args
        // only, so caching would serve one unit's supplier list to users of another unit.
        "ISupplierLookup",
    };

    // Populated by AddLookupCaching during discovery so CacheInvalidationBehavior knows
    // which interface names actually have a cache to evict (silent no-op for unknown names).
    internal static readonly HashSet<string> RegisteredCachedLookupNames =
        new(StringComparer.Ordinal);

    private static List<Type> DiscoverLookupInterfaces()
    {
        var lookupInterfaces = new List<Type>();

        // Get the Contracts assembly (where all I*Lookup interfaces are defined)
        var contractsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "Contracts");

        if (contractsAssembly != null)
        {
            lookupInterfaces.AddRange(
                contractsAssembly.GetTypes()
                    .Where(t => t.IsInterface && t.Name.EndsWith("Lookup")
                                && !ExcludedLookupInterfaces.Contains(t.Name))
                    .ToList()
            );
        }

        // Also check other assemblies for lookup interfaces
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.GetName().Name != "Contracts");

        foreach (var assembly in allAssemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsInterface &&
                                t.Namespace != null &&
                                t.Namespace.Contains("Lookups") &&
                                t.Name.Contains("Lookup") &&
                                !ExcludedLookupInterfaces.Contains(t.Name))
                    .ToList();

                lookupInterfaces.AddRange(types);
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be loaded
            }
        }

        return lookupInterfaces.Distinct().ToList();
    }

    /// <summary>
    /// Decorates a single lookup interface with the caching proxy.
    /// Uses Scrutor's Decorate functionality.
    /// </summary>
    private static void DecorateLookup(IServiceCollection services, Type lookupInterface)
    {
        // Create generic decorator type: CachedLookupDecorator<IUnitLookup>
        var decoratorType = typeof(CachedLookupDecorator<>).MakeGenericType(lookupInterface);

        // Use Scrutor to decorate the service
        services.Decorate(lookupInterface, (inner, provider) =>
        {
            var cache = provider.GetRequiredService<IMemoryCache>();
            var options = provider.GetRequiredService<LookupCacheOptions>();
            var loggerType = typeof(ILogger<>).MakeGenericType(decoratorType);
            var logger = provider.GetRequiredService(loggerType);
            var invalidator = provider.GetRequiredService<LookupCacheInvalidator>();

            // Call CachedLookupDecorator<TLookup>.Create(inner, cache, options, logger, invalidator)
            var createMethod = decoratorType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!;

            return createMethod.Invoke(null, new object[] { inner, cache, options, logger, invalidator })!;
        });
    }
}
