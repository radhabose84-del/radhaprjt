using Dapper;
using Hangfire;
using Hangfire.States;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// US-GL02-FR-008a — Auto-re-freeze. When an unfreeze window (opened by US-GL02-08B) lapses,
/// returns the COA to frozen automatically (AC3). Pure SQL UPDATE on Finance.CoaFreezeState —
/// not subject to the COA freeze triggers (those guard GlAccountMaster / AccountGroup, not this table).
/// Runs every minute in BSOFT.Worker; idempotent, so a missed run is picked up on the next tick.
/// </summary>
public class CoaAutoReFreezeJob
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CoaAutoReFreezeJob> _logger;

    public CoaAutoReFreezeJob(IConfiguration configuration, ILogger<CoaAutoReFreezeJob> logger)
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
            _logger.LogError("CoaAutoReFreezeJob: DefaultConnection is not configured.");
            return;
        }

        const string sql = @"
            UPDATE Finance.CoaFreezeState
            SET IsFrozen             = 1,
                UnfreezeWindowExpiry = NULL,
                ModifiedDate         = SYSDATETIMEOFFSET(),
                ModifiedByName       = 'System (Auto-Re-Freeze)'
            WHERE IsDeleted = 0
              AND IsFrozen  = 0
              AND UnfreezeWindowExpiry IS NOT NULL
              AND UnfreezeWindowExpiry < SYSDATETIMEOFFSET();";

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var affected = await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));

            if (affected > 0)
                _logger.LogInformation("CoaAutoReFreezeJob: auto-re-froze {Count} company COA(s) after window expiry.", affected);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "CoaAutoReFreezeJob: auto-re-freeze sweep failed; will retry next tick.");
        }
    }
}
