using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Shared.Infrastructure.Resilience.PipelineBuilders;

internal static class HttpPipelineBuilder
{
    public static ResiliencePipeline<HttpResponseMessage> Build(
        string profileName,
        ResilienceProfile profile,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var logger = loggerFactory?.CreateLogger($"BsoftResilience.Http.{profileName}");

        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(static r => HttpStatusCodeHelper.IsTransientFailure(r)),
                MaxRetryAttempts = profile.RetryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = profile.UseJitter,
                Delay = TimeSpan.FromMilliseconds(profile.BaseDelayMs),
                OnRetry = args =>
                {
                    logger?.LogWarning(
                        "HTTP retry attempt {Attempt} after {Delay}ms for profile {Profile}. Outcome: {Outcome}",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds,
                        profileName,
                        args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString() ?? "unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(static r => HttpStatusCodeHelper.IsTransientFailure(r)),
                FailureRatio = profile.BreakerFailureRatio,
                SamplingDuration = TimeSpan.FromSeconds(profile.BreakerSamplingSeconds),
                MinimumThroughput = profile.BreakerMinThroughput,
                BreakDuration = TimeSpan.FromSeconds(profile.BreakerBreakSeconds),
                OnOpened = args =>
                {
                    logger?.LogError(
                        "HTTP circuit breaker opened for {BreakDuration}s on profile {Profile}",
                        args.BreakDuration.TotalSeconds,
                        profileName);
                    return ValueTask.CompletedTask;
                },
                OnClosed = _ =>
                {
                    logger?.LogInformation(
                        "HTTP circuit breaker closed on profile {Profile}",
                        profileName);
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(TimeSpan.FromSeconds(profile.TimeoutSeconds))
            .Build();
    }
}

internal static class HttpStatusCodeHelper
{
    public static bool IsTransientFailure(HttpResponseMessage? response)
    {
        if (response is null)
        {
            return false;
        }

        var status = (int)response.StatusCode;
        return status >= 500 || status == 408 || status == 429;
    }
}
