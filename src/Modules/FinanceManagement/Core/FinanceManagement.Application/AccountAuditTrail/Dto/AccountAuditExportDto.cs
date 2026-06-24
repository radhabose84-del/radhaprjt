namespace FinanceManagement.Application.AccountAuditTrail.Dto
{
    // Export payload for a date range (US-GL02-09 AC-4). RecordCount is the tamper-evident
    // record-count checksum; Checksum is a SHA-256 over the row contents for stronger tamper evidence.
    public sealed class AccountAuditExportDto
    {
        public string? EntityName { get; set; }
        public DateTimeOffset FromDate { get; set; }
        public DateTimeOffset ToDate { get; set; }

        public int RecordCount { get; set; }
        public string Checksum { get; set; } = null!;

        public List<AccountAuditTrailDto> Rows { get; set; } = new();
    }
}
