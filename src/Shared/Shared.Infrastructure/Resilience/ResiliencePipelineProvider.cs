using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Shared.Infrastructure.Resilience.PipelineBuilders;

namespace Shared.Infrastructure.Resilience;

internal sealed class ResiliencePipelineProvider : IResiliencePipelineProvider
{
    private readonly ResilienceOptions _options;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ConcurrentDictionary<string, ResiliencePipeline> _mongoCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ResiliencePipeline> _sqlCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, ResiliencePipeline> _signalRCache = new(StringComparer.OrdinalIgnoreCase);

    public ResiliencePipelineProvider(
        IOptions<ResilienceOptions> options,
        ILoggerFactory? loggerFactory = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _loggerFactory = loggerFactory;
    }

    public ResilienceProfile GetProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new ArgumentException("Profile name is required.", nameof(profileName));
        }

        if (_options.Profiles.TryGetValue(profileName, out var profile))
        {
            return profile;
        }

        throw new InvalidOperationException(
            $"Resilience profile '{profileName}' is not configured. Add it under '{ResilienceOptions.SectionName}:Profiles' in appsettings.json.");
    }

    public ResiliencePipeline GetMongoPipeline(string profileName) =>
        _mongoCache.GetOrAdd(profileName, name =>
            GenericPipelineBuilder.Build(
                $"Mongo.{name}",
                GetProfile(name),
                IsTransientMongoException,
                _loggerFactory));

    public ResiliencePipeline GetSqlPipeline(string profileName) =>
        _sqlCache.GetOrAdd(profileName, name =>
            GenericPipelineBuilder.Build(
                $"Sql.{name}",
                GetProfile(name),
                IsTransientSqlException,
                _loggerFactory));

    public ResiliencePipeline GetSignalRPipeline(string profileName) =>
        _signalRCache.GetOrAdd(profileName, name =>
            GenericPipelineBuilder.Build(
                $"SignalR.{name}",
                GetProfile(name),
                IsTransientSignalRException,
                _loggerFactory));

    public ResiliencePipeline GetGenericPipeline(string profileName, Func<Exception, bool> isTransient) =>
        GenericPipelineBuilder.Build(
            $"Generic.{profileName}",
            GetProfile(profileName),
            isTransient,
            _loggerFactory);

    internal static bool IsTransientMongoException(Exception ex)
    {
        if (ex is TimeoutException or IOException)
        {
            return true;
        }

        var typeName = ex.GetType().Name;
        return TransientMongoTypeNames.Contains(typeName);
    }

    private static readonly HashSet<string> TransientMongoTypeNames = new(StringComparer.Ordinal)
    {
        "MongoConnectionException",
        "MongoExecutionTimeoutException",
        "MongoNotPrimaryException",
        "MongoNodeIsRecoveringException",
        "MongoConnectionPoolPausedException",
        "MongoWaitQueueFullException"
    };

    internal static bool IsTransientSqlException(Exception ex)
    {
        if (ex is TimeoutException or IOException)
        {
            return true;
        }

        if (ex is SqlException sql)
        {
            return TransientSqlErrorNumbers.Contains(sql.Number);
        }

        return false;
    }

    internal static bool IsTransientSignalRException(Exception ex) =>
        ex is HttpRequestException
            or TimeoutException
            or IOException
            or InvalidOperationException;

    private static readonly HashSet<int> TransientSqlErrorNumbers = new()
    {
        4060,
        40197,
        40501,
        40613,
        49918,
        49919,
        49920,
        11001,
        10928,
        10929,
        233,
        64,
        20
    };
}
