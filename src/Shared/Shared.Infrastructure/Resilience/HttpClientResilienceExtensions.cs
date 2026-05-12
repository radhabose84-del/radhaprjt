using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Shared.Infrastructure.Resilience.PipelineBuilders;

namespace Shared.Infrastructure.Resilience;

public static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder AddBsoftHttpResilience(
        this IHttpClientBuilder builder,
        string profileName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name is required.", nameof(profileName));
        }

        builder.AddResilienceHandler($"bsoft-http-{profileName}", (pipelineBuilder, ctx) =>
        {
            var options = ctx.ServiceProvider
                .GetRequiredService<IOptions<ResilienceOptions>>().Value;

            if (!options.Profiles.TryGetValue(profileName, out var profile))
            {
                throw new InvalidOperationException(
                    $"Resilience profile '{profileName}' is not configured. " +
                    $"Add it under '{ResilienceOptions.SectionName}:Profiles' in appsettings.json.");
            }

            var loggerFactory = ctx.ServiceProvider.GetService<ILoggerFactory>();
            var clientName = builder.Name;
            var logger = loggerFactory?.CreateLogger($"BsoftResilience.Http.{clientName}");

            var transientPredicate = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .Handle<TimeoutRejectedException>()
                .HandleResult(static r => HttpStatusCodeHelper.IsTransientFailure(r));

            pipelineBuilder
                .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = transientPredicate,
                    MaxRetryAttempts = profile.RetryCount,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = profile.UseJitter,
                    Delay = TimeSpan.FromMilliseconds(profile.BaseDelayMs),
                    OnRetry = args =>
                    {
                        logger?.LogWarning(
                            "HTTP retry attempt {Attempt} after {Delay}ms for client {Client}/{Profile}. Outcome: {Outcome}",
                            args.AttemptNumber + 1,
                            args.RetryDelay.TotalMilliseconds,
                            clientName,
                            profileName,
                            args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "unknown");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
                {
                    ShouldHandle = transientPredicate,
                    FailureRatio = profile.BreakerFailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(profile.BreakerSamplingSeconds),
                    MinimumThroughput = profile.BreakerMinThroughput,
                    BreakDuration = TimeSpan.FromSeconds(profile.BreakerBreakSeconds),
                    OnOpened = args =>
                    {
                        logger?.LogError(
                            "HTTP circuit breaker opened for {BreakDuration}s on client {Client}/{Profile}",
                            args.BreakDuration.TotalSeconds,
                            clientName,
                            profileName);
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = _ =>
                    {
                        logger?.LogInformation(
                            "HTTP circuit breaker closed on client {Client}/{Profile}",
                            clientName,
                            profileName);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(profile.TimeoutSeconds));
        });

        return builder;
    }
}
