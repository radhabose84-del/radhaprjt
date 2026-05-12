using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class GenericPipelineBehaviorTests
{
    private static IResiliencePipelineProvider BuildProvider(int retryCount = 3, int timeoutSeconds = 30)
    {
        var options = new ResilienceOptions();
        options.Profiles["Test"] = new ResilienceProfile
        {
            RetryCount             = retryCount,
            BaseDelayMs            = 1,
            TimeoutSeconds         = timeoutSeconds,
            BreakerFailureRatio    = 0.99,
            BreakerSamplingSeconds = 30,
            BreakerMinThroughput   = 1000,
            BreakerBreakSeconds    = 30,
            UseJitter              = false
        };
        return new ResiliencePipelineProvider(Options.Create(options));
    }

    [Fact]
    public async Task Pipeline_RetriesTransientException_UntilSuccess()
    {
        var provider = BuildProvider(retryCount: 3);
        var pipeline = provider.GetGenericPipeline("Test", ex => ex is InvalidOperationException);

        var attempts = 0;
        await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            if (attempts < 3) throw new InvalidOperationException("transient");
            return ValueTask.CompletedTask;
        });

        attempts.Should().Be(3);
    }

    [Fact]
    public async Task Pipeline_DoesNotRetry_NonTransientException()
    {
        var provider = BuildProvider(retryCount: 5);
        var pipeline = provider.GetGenericPipeline("Test", ex => ex is InvalidOperationException);

        var attempts = 0;
        Func<Task> act = async () => await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            throw new ArgumentException("not transient");
        });

        await act.Should().ThrowAsync<ArgumentException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task Pipeline_StopsAfterMaxRetries()
    {
        var provider = BuildProvider(retryCount: 2);
        var pipeline = provider.GetGenericPipeline("Test", ex => ex is InvalidOperationException);

        var attempts = 0;
        Func<Task> act = async () => await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            throw new InvalidOperationException("always fails");
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
        attempts.Should().Be(3); // 1 initial + 2 retries
    }

    [Fact]
    public async Task Pipeline_TimesOut_OnLongRunningOperation()
    {
        // Use a non-matching predicate so the retry strategy never engages, while still
        // satisfying Polly v8's MaxRetryAttempts >= 1 validation.
        var provider = BuildProvider(retryCount: 1, timeoutSeconds: 1);
        var pipeline = provider.GetGenericPipeline("Test", ex => ex is FormatException);

        Func<Task> act = async () => await pipeline.ExecuteAsync(async ct =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        });

        await act.Should().ThrowAsync<TimeoutRejectedException>();
    }

    [Fact]
    public async Task SqlPipeline_OnTimeoutException_Retries()
    {
        var provider = BuildProvider(retryCount: 2);
        var pipeline = provider.GetSqlPipeline("Test");

        var attempts = 0;
        Func<Task> act = async () => await pipeline.ExecuteAsync(_ =>
        {
            attempts++;
            throw new TimeoutException("sql timeout");
        });

        await act.Should().ThrowAsync<TimeoutException>();
        attempts.Should().Be(3); // 1 initial + 2 retries
    }
}
