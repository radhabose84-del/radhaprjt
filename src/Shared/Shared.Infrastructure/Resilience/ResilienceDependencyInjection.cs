using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shared.Infrastructure.Resilience;

public static class ResilienceDependencyInjection
{
    public static IServiceCollection AddBsoftResilience(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<ResilienceOptions>()
            .Bind(configuration.GetSection(ResilienceOptions.SectionName))
            .PostConfigure(EnsureDefaultProfiles);

        services.TryAddSingleton<IResiliencePipelineProvider, ResiliencePipelineProvider>();

        return services;
    }

    internal static void EnsureDefaultProfiles(ResilienceOptions options)
    {
        options.Profiles ??= new Dictionary<string, ResilienceProfile>(StringComparer.OrdinalIgnoreCase);

        if (!options.Profiles.ContainsKey(ResilienceProfileNames.Standard))
        {
            options.Profiles[ResilienceProfileNames.Standard] = new ResilienceProfile
            {
                RetryCount = 3,
                BaseDelayMs = 1000,
                TimeoutSeconds = 30,
                BreakerFailureRatio = 0.5,
                BreakerSamplingSeconds = 30,
                BreakerMinThroughput = 8,
                BreakerBreakSeconds = 30,
                UseJitter = true
            };
        }

        if (!options.Profiles.ContainsKey(ResilienceProfileNames.Critical))
        {
            options.Profiles[ResilienceProfileNames.Critical] = new ResilienceProfile
            {
                RetryCount = 5,
                BaseDelayMs = 2000,
                TimeoutSeconds = 60,
                BreakerFailureRatio = 0.4,
                BreakerSamplingSeconds = 60,
                BreakerMinThroughput = 10,
                BreakerBreakSeconds = 60,
                UseJitter = true
            };
        }

        if (!options.Profiles.ContainsKey(ResilienceProfileNames.FastFail))
        {
            options.Profiles[ResilienceProfileNames.FastFail] = new ResilienceProfile
            {
                RetryCount = 1,
                BaseDelayMs = 500,
                TimeoutSeconds = 5,
                BreakerFailureRatio = 0.7,
                BreakerSamplingSeconds = 15,
                BreakerMinThroughput = 6,
                BreakerBreakSeconds = 15,
                UseJitter = true
            };
        }
    }
}
