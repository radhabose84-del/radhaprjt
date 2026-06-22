using Dapper;
using Hangfire;
using Hangfire.States;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// US-GL02-08B (AC4) — when an unfreeze window expires with incomplete change requests, cancel those
/// requests as 'Lapsed' and close the window as 'Expired'. Separate from the 08A auto-re-freeze job (which
/// re-seals Finance.CoaFreezeState); both run every minute and are idempotent, so the brief skew between
/// re-freeze and lapse is self-correcting. Pure SQL — not subject to the COA freeze triggers.
/// </summary>
public class CoaLapseExpiredRequestsJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CoaLapseExpiredRequestsJob> _logger;

    public CoaLapseExpiredRequestsJob(IConfiguration configuration, ILogger<CoaLapseExpiredRequestsJob> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [Queue("coa-refreeze-queue")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var connectionString = (_configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
            .Replace("{SERVER}",       Environment.GetEnvironmentVariable("DATABASE_SERVER")   ?? "")
            .Replace("{USER_ID}",      Environment.GetEnvironmentVariable("DATABASE_USERID")   ?? "")
            .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("CoaLapseExpiredRequestsJob: DefaultConnection is not configured.");
            return;
        }

        // 1) Lapse not-yet-committed change requests under an expired window.
        // 2) Close the expired window itself. (Order matters: lapse while the window is still 'WindowOpen'.)
        const string sql = @"
            UPDATE c
            SET c.Status         = 'Lapsed',
                c.ModifiedDate   = SYSDATETIMEOFFSET(),
                c.ModifiedByName = 'System (Lapse)'
            FROM Finance.CoaChangeRequest c
            JOIN Finance.CoaUnfreezeRequest u ON u.Id = c.UnfreezeRequestId
            WHERE c.IsDeleted = 0
              AND u.IsDeleted = 0
              AND c.Status = 'ImpactApproved'
              AND u.Status = 'WindowOpen'
              AND u.WindowExpiry IS NOT NULL
              AND u.WindowExpiry < SYSDATETIMEOFFSET();

            UPDATE u
            SET u.Status         = 'Expired',
                u.ModifiedDate   = SYSDATETIMEOFFSET(),
                u.ModifiedByName = 'System (Lapse)'
            FROM Finance.CoaUnfreezeRequest u
            WHERE u.IsDeleted = 0
              AND u.Status = 'WindowOpen'
              AND u.WindowExpiry IS NOT NULL
              AND u.WindowExpiry < SYSDATETIMEOFFSET();";

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var affected = await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

            if (affected > 0)
                _logger.LogInformation("CoaLapseExpiredRequestsJob: lapsed/closed {Count} row(s) after window expiry.", affected);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "CoaLapseExpiredRequestsJob: lapse sweep failed; will retry next tick.");
        }
    }
}
