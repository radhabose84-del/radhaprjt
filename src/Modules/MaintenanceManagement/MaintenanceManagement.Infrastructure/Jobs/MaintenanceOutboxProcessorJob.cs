using System.Text.Json;
using Contracts.Dtos.Maintenance.Preventive;
using Contracts.Events.Maintenance;
using Contracts.Events.Maintenance.PreventiveScheduler;
using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
using Dapper;
using Hangfire;
using Hangfire.States;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Infrastructure.Jobs;

/// <summary>
/// Hangfire job that runs in BSOFT.Api (maintenance-jobs queue).
/// Polls maintenance.OutboxMessages for scheduling events
/// (MachineWiseScheduleCreationEvent, HeaderUpdateEvent, NextSchedulerCreatedEvent)
/// and schedules ScheduleWorkOrderJob directly — no RabbitMQ hop.
///
/// SqlOutboxProcessorJob (BSOFT.Worker) skips these three event types
/// so there is no double-processing.
/// </summary>
public class MaintenanceOutboxProcessorJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MaintenanceOutboxProcessorJob> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int BatchSize = 50;

    public MaintenanceOutboxProcessorJob(
        IConfiguration configuration,
        ILogger<MaintenanceOutboxProcessorJob> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [Queue("maintenance-jobs")]
    [DisableConcurrentExecution(10 * 60)]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var connectionString = (_configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
            .Replace("{SERVER}",       Environment.GetEnvironmentVariable("DATABASE_SERVER")   ?? "")
            .Replace("{USER_ID}",      Environment.GetEnvironmentVariable("DATABASE_USERID")   ?? "")
            .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("MaintenanceOutboxProcessorJob: DefaultConnection is not configured.");
            return;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var querySql = $"""
            SELECT TOP ({BatchSize})
                Id, EventType, EventData, RetryCount, MaxRetries
            FROM [maintenance].[OutboxMessages]
            WHERE Status = 0
              AND (NextRetryAt IS NULL OR NextRetryAt <= @Now)
              AND RetryCount < MaxRetries
              AND ProcessorHint = 'maintenance'
            ORDER BY CreatedAt ASC
            """;

        var messages = (await connection.QueryAsync<OutboxRow>(querySql, new { Now = DateTimeOffset.UtcNow })).AsList();

        if (messages.Count == 0)
            return;

        _logger.LogInformation(
            "MaintenanceOutboxProcessorJob: {Count} scheduling message(s) to process.", messages.Count);

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await ProcessMessageAsync(connection, message);
        }
    }

    // ─── Per-message dispatcher ──────────────────────────────────────────────────

    private async Task ProcessMessageAsync(SqlConnection connection, OutboxRow message)
    {
        try
        {
            if (message.EventType.Contains("MachineWiseScheduleCreationEvent"))
                await ProcessMachineWiseScheduleCreationAsync(connection, message);
            else if (message.EventType.Contains("HeaderUpdateEvent"))
                await ProcessHeaderUpdateAsync(connection, message);
            else if (message.EventType.Contains("NextSchedulerCreatedEvent"))
                await ProcessNextSchedulerCreatedAsync(connection, message);

            await MarkPublishedAsync(connection, message.Id);

            _logger.LogInformation(
                "MaintenanceOutboxProcessorJob: Processed message {Id} ({EventType}).",
                message.Id, message.EventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "MaintenanceOutboxProcessorJob: Failed to process message {Id}.", message.Id);
            await MarkFailedAsync(connection, message, ex.Message);
        }
    }

    // ─── Event handlers ──────────────────────────────────────────────────────────

    /// <summary>
    /// CreatePreventiveScheduler publishes this event.
    /// Schedule one ScheduleWorkOrderJob per machine detail.
    /// </summary>
    private async Task ProcessMachineWiseScheduleCreationAsync(SqlConnection connection, OutboxRow message)
    {
        var @event = JsonSerializer.Deserialize<MachineWiseScheduleCreationEvent>(message.EventData, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize MachineWiseScheduleCreationEvent.");

        foreach (var detail in @event.ScheduleDetail ?? Enumerable.Empty<ScheduleDetailSagaDto>())
        {
            var delayInMinutes = detail.DelayInMinutes < 5 ? 5 : detail.DelayInMinutes;

            var jobId = BackgroundJob.Schedule<ScheduleWorkOrderJob>(
                job => job.ExecuteAsync(detail.Id),
                TimeSpan.FromMinutes(delayInMinutes));

            await UpdateDetailJobIdAsync(connection, detail.Id, jobId);

            _logger.LogInformation(
                "MaintenanceOutboxProcessorJob: Scheduled job {JobId} for DetailId {DetailId} (delay {Delay} min).",
                jobId, detail.Id, delayInMinutes);
        }
    }

    /// <summary>
    /// UpdatePreventiveScheduler publishes this event when frequency changes.
    /// Remove old Hangfire jobs and schedule new ones with updated delays.
    /// </summary>
    private async Task ProcessHeaderUpdateAsync(SqlConnection connection, OutboxRow message)
    {
        var @event = JsonSerializer.Deserialize<HeaderUpdateEvent>(message.EventData, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize HeaderUpdateEvent.");

        foreach (var detail in @event.ScheduleDetailUpdate ?? Enumerable.Empty<ScheduleDetailUpdateDto>())
        {
            // Fetch current HangfireJobId from DB to cancel the old scheduled job
            var oldJobId = await connection.ExecuteScalarAsync<string?>(
                "SELECT HangfireJobId FROM [maintenance].[PreventiveSchedulerDetail] WHERE Id = @Id",
                new { detail.Id });

            if (!string.IsNullOrWhiteSpace(oldJobId))
                BackgroundJob.Delete(oldJobId);

            var delayInMinutes = detail.DelayInMinutes < 5 ? 5 : detail.DelayInMinutes;

            var newJobId = BackgroundJob.Schedule<ScheduleWorkOrderJob>(
                job => job.ExecuteAsync(detail.Id),
                TimeSpan.FromMinutes(delayInMinutes));

            await UpdateDetailJobIdAsync(connection, detail.Id, newJobId);

            _logger.LogInformation(
                "MaintenanceOutboxProcessorJob: Rescheduled job {JobId} for DetailId {DetailId} (delay {Delay} min).",
                newJobId, detail.Id, delayInMinutes);
        }
    }

    /// <summary>
    /// UpdateWorkOrder (completion) publishes this event.
    /// Schedule the next preventive maintenance cycle.
    /// </summary>
    private async Task ProcessNextSchedulerCreatedAsync(SqlConnection connection, OutboxRow message)
    {
        var @event = JsonSerializer.Deserialize<NextSchedulerCreatedEvent>(message.EventData, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize NextSchedulerCreatedEvent.");

        var delayInMinutes = @event.DelayInMinutes < 5 ? 5 : @event.DelayInMinutes;

        var jobId = BackgroundJob.Schedule<ScheduleWorkOrderJob>(
            job => job.ExecuteAsync(@event.PreventiveSchedulerDetailId),
            TimeSpan.FromMinutes(delayInMinutes));

        await UpdateDetailJobIdAsync(connection, @event.PreventiveSchedulerDetailId, jobId);

        _logger.LogInformation(
            "MaintenanceOutboxProcessorJob: Scheduled next-cycle job {JobId} for DetailId {DetailId} (delay {Delay} min).",
            jobId, @event.PreventiveSchedulerDetailId, delayInMinutes);
    }

    // ─── SQL helpers ─────────────────────────────────────────────────────────────

    private static async Task UpdateDetailJobIdAsync(SqlConnection connection, int detailId, string jobId)
    {
        await connection.ExecuteAsync(
            "UPDATE [maintenance].[PreventiveSchedulerDetail] SET HangfireJobId = @JobId WHERE Id = @Id",
            new { JobId = jobId, Id = detailId });
    }

    private static async Task MarkPublishedAsync(SqlConnection connection, long id)
    {
        await connection.ExecuteAsync(
            "UPDATE [maintenance].[OutboxMessages] SET Status = 1, PublishedAt = @Now, LastError = NULL WHERE Id = @Id",
            new { Now = DateTimeOffset.UtcNow, Id = id });
    }

    private static async Task MarkFailedAsync(SqlConnection connection, OutboxRow message, string error)
    {
        var newRetryCount = message.RetryCount + 1;
        var nextRetryAt   = DateTimeOffset.UtcNow.AddSeconds(Math.Pow(2, newRetryCount));
        var truncated     = error.Length > 2000 ? error[..2000] : error;
        var newStatus     = newRetryCount >= message.MaxRetries ? 2 : 0;

        await connection.ExecuteAsync("""
            UPDATE [maintenance].[OutboxMessages]
            SET RetryCount  = @RetryCount,
                LastError   = @Error,
                NextRetryAt = @NextRetryAt,
                Status      = @Status
            WHERE Id = @Id
            """,
            new { RetryCount = newRetryCount, Error = truncated, NextRetryAt = nextRetryAt, Status = newStatus, Id = message.Id });
    }

    // ─── Dapper projection ───────────────────────────────────────────────────────

    private sealed record OutboxRow(
        long   Id,
        string EventType,
        string EventData,
        int    RetryCount,
        int    MaxRetries);
}
