namespace FinanceManagement.Domain.Entities
{
    // US-GL01-10 — records any attempted UPDATE/DELETE on a posted journal. Written by the DB triggers
    // (not application code). Deliberately minimal and FK-free so the trigger insert never fails.
    public class SecurityViolationLog
    {
        public int Id { get; set; }

        public string? TableName { get; set; }          // JournalHeader / JournalDetail
        public int? JournalHeaderId { get; set; }       // affected voucher (no FK — row may be mid-delete)
        public string? AttemptedAction { get; set; }    // UPDATE / DELETE
        public string? UserName { get; set; }           // SUSER_SNAME()
        public DateTimeOffset AttemptedAt { get; set; } // SYSDATETIMEOFFSET()
        public string? Channel { get; set; }            // DB / API
    }
}
