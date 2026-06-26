namespace FinanceManagement.Application.JournalMaster.Dto
{
    /// <summary>
    /// US-GL03-04 / AC#3 — one row of the late-posting report. Feeds the close-review screen
    /// and the weekly CFO digest. Backed by the Finance.JournalHeader filtered index on IsBackdated.
    /// </summary>
    public class LatePostingReportDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }

        public int VoucherTypeId { get; set; }
        public string? VoucherTypeName { get; set; }
        public string? VoucherNo { get; set; }
        public DateOnly VoucherDate { get; set; }

        public int? AccountingPeriodId { get; set; }
        public string? AccountingPeriodName { get; set; }
        public string? AccountingPeriodStatusCode { get; set; }

        public int StatusId { get; set; }
        public string? StatusCode { get; set; }              // JOURNAL_STATUS code (POSTED / APPROVED / …)
        public string? StatusName { get; set; }

        public DateTimeOffset? PostedAt { get; set; }
        public string? PostedBy { get; set; }

        /// <summary>Always true on this report (filter clause). Surfaced for symmetry.</summary>
        public bool IsBackdated { get; set; }

        /// <summary>PostedAt date minus VoucherDate (in days). Useful for "stale" sort.</summary>
        public int DaysBackdated { get; set; }

        public string? BackdateReason { get; set; }
        public int? BackdateAcknowledgedBy { get; set; }
        public DateTimeOffset? BackdateAcknowledgedAt { get; set; }

        public decimal TotalDr { get; set; }
        public decimal TotalCr { get; set; }
        public string? Narration { get; set; }

        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
    }
}
