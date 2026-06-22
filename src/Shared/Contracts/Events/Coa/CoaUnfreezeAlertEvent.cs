namespace Contracts.Events.Coa
{
    // US-GL02-08B (AC2) — raised when a dual-approval unfreeze window opens. Carries the pre-resolved
    // recipient emails (CFO / FC / Internal Audit), so the BackgroundService consumer just sends — no
    // notification-config/SP seeding required. Published through the Finance transactional outbox.
    public class CoaUnfreezeAlertEvent
    {
        public Guid CorrelationId { get; set; }
        public int CompanyId { get; set; }
        public int UnfreezeRequestId { get; set; }
        public string? Reason { get; set; }
        public int CfoApproverUserId { get; set; }
        public int SysAdminApproverUserId { get; set; }
        public DateTimeOffset WindowExpiry { get; set; }
        public List<string> RecipientEmails { get; set; } = new();
    }
}
