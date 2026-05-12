using Polly;

namespace Shared.Infrastructure.Resilience;

public static class DapperResilienceExtensions
{
    public static ValueTask<T> ExecuteSqlAsync<T>(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetSqlPipeline(profileName);
        return pipeline.ExecuteAsync(async ct => await operation(ct).ConfigureAwait(false), cancellationToken);
    }

    public static ValueTask ExecuteSqlAsync(
        this IResiliencePipelineProvider provider,
        string profileName,
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(operation);

        var pipeline = provider.GetSqlPipeline(profileName);
        return pipeline.ExecuteAsync(async ct => await operation(ct).ConfigureAwait(false), cancellationToken);
    }
}
