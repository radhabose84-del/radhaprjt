#nullable enable
using System.Reflection;
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

        // 3️⃣ Auto-discover ALL I*Lookup interfaces from Contracts assembly
        var lookupInterfaces = DiscoverLookupInterfaces();

        // 4️⃣ Decorate each lookup interface with caching proxy
        foreach (var lookupInterface in lookupInterfaces)
        {
            DecorateLookup(services, lookupInterface);
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
    private static readonly HashSet<string> ExcludedLookupInterfaces = new(StringComparer.Ordinal)
    {
        // IDocumentSequenceLookup has both write operations (IncrementDocNoAsync) and
        // frequently-changing reads (GenerateDocumentNumber changes after every doc created).
        // Caching either causes duplicate document numbers.
        "IDocumentSequenceLookup",
    };

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

            // Call CachedLookupDecorator<TLookup>.Create(inner, cache, options, logger)
            var createMethod = decoratorType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!;

            return createMethod.Invoke(null, new object[] { inner, cache, options, logger })!;
        });
    }
}
