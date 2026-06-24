namespace FinanceManagement.Domain.Entities
{
    // US-GL01-03B — records each scheduled scan of a voucher number series for missing numbers.
    // Lean log (NOT a BaseEntity — job-written, no audit context).
    public class SequenceGapScanLog
    {
        public int Id { get; set; }

        public int SeriesId { get; set; }               // same-module FK -> VoucherTypeNumberSeries
        public DateTimeOffset ScannedAt { get; set; }
        public int GapsFound { get; set; }
        public string? GapNumbers { get; set; }         // CSV/JSON of missing numbers
        public bool Alerted { get; set; }

        // Same-module FK navigation
        public VoucherTypeNumberSeries? Series { get; set; }
    }
}
