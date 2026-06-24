namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan
{
    // US-GL01-03B — invoked by the periodic Hangfire job to scan every number series for gaps.
    // Returns the total number of missing numbers found across all series.
    public interface IGapScanService
    {
        Task<int> ScanAllAsync(CancellationToken ct);
    }
}
