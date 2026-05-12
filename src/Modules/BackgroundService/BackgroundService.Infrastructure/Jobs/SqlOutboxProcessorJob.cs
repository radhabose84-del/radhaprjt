using System.Reflection;
using System.Text.Json;
using Dapper;
using Hangfire;
using Hangfire.States;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Resilience;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// Centralized Hangfire job that polls SQL outbox tables across all modules
/// and publishes pending events to RabbitMQ via MassTransit.
///
/// Runs in BSOFT.Worker (and can also run in BSOFT.Api's Hangfire server if
/// that process picks up the sql-outbox-queue). Hangfire's distributed locking
/// guarantees only one server executes the job at a time.
/// </summary>
public class SqlOutboxProcessorJob
{
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<SqlOutboxProcessorJob> _logger;
    private readonly IResiliencePipelineProvider _resilience;

    /// <summary>All SQL outbox tables to poll, keyed by (schema, table).</summary>
    private static readonly (string Schema, string Table)[] OutboxTables =
    [
        ("purchase",    "OutboxMessages"),
        ("maintenance", "OutboxMessages"),
        ("Budget",      "OutboxMessages"),
        ("Inventory",   "OutboxMessages"),
        ("Party",       "OutboxMessages"),
        ("Project",     "OutboxMessages"),
  ("Sales",       "OutboxMessages"),
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int BatchSize    = 100;
    private const int RetentionDays = 7;

    public SqlOutboxProcessorJob(
        IConfiguration configuration,
        IPublishEndpoint publishEndpoint,
        ILogger<SqlOutboxProcessorJob> logger,
        IResiliencePipelineProvider resilience)
    {
        _configuration  = configuration;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
        _resilience      = resilience;
    }

    /// <summary>
    /// Entry point invoked by Hangfire every minute.
    /// Opens a single SQL connection and processes all registered outbox tables.
    /// </summary>
    [Queue("sql-outbox-queue")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var connectionString = (_configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
            .Replace("{SERVER}",       Environment.GetEnvironmentVariable("DATABASE_SERVER")   ?? "")
            .Replace("{USER_ID}",      Environment.GetEnvironmentVariable("DATABASE_USERID")   ?? "")
            .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("SqlOutboxProcessorJob: DefaultConnection is not configured.");
            return;
        }

        foreach (var (schema, table) in OutboxTables)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Fresh connection per table — if one table's processing causes a connection
            // failure (timeout, broken pipe), subsequent tables are not affected
            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                await ProcessTableAsync(connection, schema, table, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex,
                    "SqlOutboxProcessorJob: Failed to process [{Schema}].[{Table}]. Continuing with next table.",
                    schema, table);
            }
        }
    }

    // ─── Private helpers ────────────────────────────────────────────────────────

    private async Task ProcessTableAsync(
        SqlConnection connection,
        string schema,
        string table,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Maintenance scheduling events carry ProcessorHint = 'maintenance' and are handled by
        // MaintenanceOutboxProcessorJob (BSOFT.Api, maintenance-jobs queue).
        // For the maintenance schema, only pick up rows where ProcessorHint IS NULL
        // (i.e. non-scheduling events that go through MassTransit).
        var maintenanceExclusion = schema.Equals("maintenance", StringComparison.OrdinalIgnoreCase)
            ? "AND ProcessorHint IS NULL"
            : string.Empty;

        var querySql = $"""
            SELECT TOP ({BatchSize})
                Id, CorrelationId, EventType, EventData, RetryCount, MaxRetries
            FROM  [{schema}].[{table}]
            WHERE Status = 0
              AND (NextRetryAt IS NULL OR NextRetryAt <= @Now)
              AND RetryCount < MaxRetries
            {maintenanceExclusion}
            ORDER BY CreatedAt ASC
            """;

        var messages = (await connection.QueryAsync<OutboxRow>(querySql, new { Now = now })).AsList();

        if (messages.Count == 0)
            return;

        _logger.LogDebug(
            "SqlOutboxProcessorJob: {Count} pending message(s) in [{Schema}].[{Table}]",
            messages.Count, schema, table);

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await ProcessSingleMessageAsync(connection, schema, table, message, cancellationToken);
        }

        // Hourly cleanup of published messages beyond the retention window
        if (now.Minute == 0)
            await CleanupOldMessagesAsync(connection, schema, table, now);
    }

    private async Task ProcessSingleMessageAsync(
        SqlConnection connection,
        string schema,
        string table,
        OutboxRow message,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventType = ResolveEventType(message.EventType);
            if (eventType == null)
            {
                _logger.LogError(
                    "SqlOutboxProcessorJob: Unknown event type '{EventType}'. MessageId: {Id}",
                    message.EventType, message.Id);

                await MarkFailedAsync(connection, schema, table, message,
                    $"Unknown event type: {message.EventType}");
                return;
            }

            var @event = JsonSerializer.Deserialize(message.EventData, eventType, JsonOptions);
            if (@event == null)
            {
                _logger.LogError(
                    "SqlOutboxProcessorJob: Deserialization returned null. MessageId: {Id}", message.Id);

                await MarkFailedAsync(connection, schema, table, message,
                    "Deserialization returned null");
                return;
            }

            await _publishEndpoint.Publish(@event, eventType, cancellationToken);
            await MarkPublishedAsync(connection, schema, table, message.Id);

            _logger.LogInformation(
                "SqlOutboxProcessorJob: Published {Id} from [{Schema}].[{Table}] " +
                "(Type: {EventType}, CorrelationId: {CorrelationId})",
                message.Id, schema, table, eventType.Name, message.CorrelationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "SqlOutboxProcessorJob: Failed to publish {Id} from [{Schema}].[{Table}]",
                message.Id, schema, table);

            await MarkFailedAsync(connection, schema, table, message, ex.Message);
        }
    }

    private async Task MarkPublishedAsync(
        SqlConnection connection, string schema, string table, long id)
    {
        var sql = $"""
            UPDATE [{schema}].[{table}]
            SET Status = 1, PublishedAt = @Now, LastError = NULL
            WHERE Id = @Id
            """;
        await _resilience.ExecuteSqlAsync(
            ResilienceProfileNames.Standard,
            _ => connection.ExecuteAsync(sql, new { Now = DateTimeOffset.UtcNow, Id = id }));
    }

    private async Task MarkFailedAsync(
        SqlConnection connection,
        string schema,
        string table,
        OutboxRow message,
        string error)
    {
        var newRetryCount  = message.RetryCount + 1;
        var delaySeconds   = Math.Pow(2, newRetryCount);
        var nextRetryAt    = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
        var truncatedError = error.Length > 2000 ? error[..2000] : error;
        var newStatus      = newRetryCount >= message.MaxRetries ? 2 : 0;

        var sql = $"""
            UPDATE [{schema}].[{table}]
            SET RetryCount  = @RetryCount,
                LastError   = @Error,
                NextRetryAt = @NextRetryAt,
                Status      = @Status
            WHERE Id = @Id
            """;

        await _resilience.ExecuteSqlAsync(
            ResilienceProfileNames.Standard,
            _ => connection.ExecuteAsync(sql, new
            {
                RetryCount  = newRetryCount,
                Error       = truncatedError,
                NextRetryAt = nextRetryAt,
                Status      = newStatus,
                Id          = message.Id,
            }));
    }

    private async Task CleanupOldMessagesAsync(
        SqlConnection connection, string schema, string table, DateTimeOffset now)
    {
        var cutoff = now.AddDays(-RetentionDays);
        var sql = $"""
            DELETE FROM [{schema}].[{table}]
            WHERE Status = 1 AND PublishedAt < @Cutoff
            """;

        var deleted = await _resilience.ExecuteSqlAsync(
            ResilienceProfileNames.Standard,
            _ => connection.ExecuteAsync(sql, new { Cutoff = cutoff }));
        if (deleted > 0)
            _logger.LogInformation(
                "SqlOutboxProcessorJob: Deleted {Count} old messages from [{Schema}].[{Table}]",
                deleted, schema, table);
    }

    /// <summary>
    /// Resolves a CLR <see cref="Type"/> from its assembly-qualified name.
    /// Tries direct resolution first, then searches all loaded assemblies,
    /// then attempts an explicit assembly load as a last resort.
    /// </summary>
    private static Type? ResolveEventType(string? assemblyQualifiedName)
    {
        if (string.IsNullOrWhiteSpace(assemblyQualifiedName))
            return null;

        var type = Type.GetType(assemblyQualifiedName);
        if (type != null) return type;

        var parts = assemblyQualifiedName.Split(',');
        if (parts.Length < 2) return null;

        var fullTypeName = parts[0].Trim();
        var assemblyName = parts[1].Trim();

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) == true)
            {
                type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
        }

        try
        {
            var loaded = Assembly.Load(assemblyName);
            type = loaded.GetType(fullTypeName);
            if (type != null) return type;
        }
        catch { /* assembly not found or cannot be loaded */ }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(fullTypeName);
            if (type != null) return type;
        }

        return null;
    }

    // ─── Dapper projection ──────────────────────────────────────────────────────

    private sealed record OutboxRow(
        long   Id,
        Guid   CorrelationId,
        string EventType,
        string EventData,
        int    RetryCount,
        int    MaxRetries);
}
