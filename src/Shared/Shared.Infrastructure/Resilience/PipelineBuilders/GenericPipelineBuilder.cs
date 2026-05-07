using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Shared.Infrastructure.Resilience.PipelineBuilders;

internal static class GenericPipelineBuilder
{
    public static ResiliencePipeline Build(
        string profileName,
        ResilienceProfile profile,
        Func<Exception, bool> isTransient,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(isTransient);

        var logger = loggerFactory?.CreateLogger($"BsoftResilience.Generic.{profileName}");

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(isTransient),
                MaxRetryAttempts = profile.RetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = profile.UseJitter,
                Delay = TimeSpan.FromMilliseconds(profile.BaseDelayMs),
                OnRetry = args =>
                {
                    logger?.LogWarning(
                        "Retry attempt {Attempt} after {Delay}ms for profile {Profile}. Exception: {Exception}",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds,
                        profileName,
                        args.Outcome.Exception?.Message ?? "unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(isTransient),
                FailureRatio = profile.BreakerFailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(profile.BreakerSamplingSeconds),
                MinimumThroughput = profile.BreakerMinThroughput,
                BreakDuration = TimeSpan.FromSeconds(profile.BreakerBreakSeconds),
                OnOpened = args =>
                {
                    logger?.LogError(
                        "Circuit breaker opened for {BreakDuration}s on profile {Profile}",
                        args.BreakDuration.TotalSeconds,
                        profileName);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(profile.TimeoutSeconds))
            .Build();
    }
}
