using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Infrastructure.Jobs;

/// <summary>
/// US-GL01-03B — nightly voucher-number gap scan. Delegates to <see cref="IGapScanService"/>, which scans
/// every active number series for missing numbers and writes a Finance.SequenceGapScanLog row per series.
/// Idempotent (it just re-records the current state), so a missed run is harmless and picked up next night.
/// Lives in BackgroundService.Infrastructure (not in a module / not in BSOFT.Worker) so the Hangfire host
/// (BSOFT.Api) can resolve the job type from its loaded assemblies.
/// </summary>
public class JournalGapScanJob
{
    private readonly IGapScanService _gapScanService;
    private readonly ILogger<JournalGapScanJob> _logger;

    public JournalGapScanJob(IGapScanService gapScanService, ILogger<JournalGapScanJob> logger)
    {
        _gapScanService = gapScanService;
        _logger = logger;
    }

    [Queue("journal-jobs-queue")]
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var gaps = await _gapScanService.ScanAllAsync(cancellationToken);
        _logger.LogInformation("JournalGapScanJob: scan complete — {Gaps} missing voucher number(s) found.", gaps);
    }
}
