namespace FinanceManagement.Application.JournalMaster.Dto
{
    // US-GL01-10 — read model for the tamper-attempt log (written by the DB immutability triggers).
    public class SecurityViolationLogDto
    {
        public int Id { get; set; }
        public string? TableName { get; set; }
        public int? JournalHeaderId { get; set; }
        public string? AttemptedAction { get; set; }
        public string? UserName { get; set; }
        public DateTimeOffset AttemptedAt { get; set; }
        public string? Channel { get; set; }
    }
}
