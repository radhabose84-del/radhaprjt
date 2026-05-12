using Polly;

namespace Shared.Infrastructure.Resilience;

public static class MongoResilienceExtensions
{
    public static ValueTask ExecuteMongoAsync(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, ValueTask> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetMongoPipeline(profileName);
        return pipeline.ExecuteAsync(operation, cancellationToken);
    }

    public static ValueTask<T> ExecuteMongoAsync<T>(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, ValueTask<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetMongoPipeline(profileName);
        return pipeline.ExecuteAsync(operation, cancellationToken);
    }

    public static ValueTask ExecuteMongoTaskAsync(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetMongoPipeline(profileName);
        return pipeline.ExecuteAsync(async ct => await operation(ct).ConfigureAwait(false), cancellationToken);
    }

    public static ValueTask<T> ExecuteMongoTaskAsync<T>(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetMongoPipeline(profileName);
        return pipeline.ExecuteAsync(async ct => await operation(ct).ConfigureAwait(false), cancellationToken);
    }
}
