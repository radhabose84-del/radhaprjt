using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Services
{
    // US-GL01-03B — scans each number series for missing numbers (1..LastUsedNumber vs the numbers
    // actually assigned on posted vouchers) and records a SequenceGapScanLog row. A clean series logs
    // GapsFound = 0, Alerted = false (no false alerts). Alert delivery (SignalR + email) is deferred.
    public class GapScanService : IGapScanService
    {
        private readonly IGapScanRepository _repository;
        private readonly ITimeZoneService _timeZoneService;

        public GapScanService(IGapScanRepository repository, ITimeZoneService timeZoneService)
        {
            _repository = repository;
            _timeZoneService = timeZoneService;
        }

        public async Task<int> ScanAllAsync(CancellationToken ct)
        {
            var series = await _repository.GetActiveSeriesAsync(ct);
            var now = _timeZoneService.GetCurrentTime();
            var totalGaps = 0;

            foreach (var s in series)
            {
                var vouchers = await _repository.GetUsedVoucherNumbersAsync(s.VoucherTypeId, s.FinancialYearId, ct);

                var used = new HashSet<int>();
                foreach (var v in vouchers)
                {
                    var n = ParseSuffix(v);
                    if (n.HasValue)
                        used.Add(n.Value);
                }

                var gaps = new List<int>();
                for (var i = 1; i <= s.LastUsedNumber; i++)
                {
                    if (!used.Contains(i))
                        gaps.Add(i);
                }

                await _repository.AddScanLogAsync(new SequenceGapScanLog
                {
                    SeriesId = s.SeriesId,
                    ScannedAt = now,
                    GapsFound = gaps.Count,
                    GapNumbers = gaps.Count > 0 ? string.Join(",", gaps) : null,
                    Alerted = gaps.Count > 0
                }, ct);

                // TODO (infra): on gaps.Count > 0, push SignalR + email alert within the 1-hour SLA.
                totalGaps += gaps.Count;
            }

            return totalGaps;
        }

        private static int? ParseSuffix(string? voucherNo)
        {
            if (string.IsNullOrWhiteSpace(voucherNo))
                return null;

            var segment = voucherNo.Split('/').Last();
            return int.TryParse(segment, out var n) ? n : null;
        }
    }
}
