namespace Contracts.Events.Coa
{
    // US-GL02-16 (AC3) — raised when a GL account transitions active -> inactive. Downstream modules
    // (AP / AR / FA) consume this to drop the account from their pick-lists / posting targets.
    // Published within 1s: direct bus publish on the happy path, Finance transactional outbox as the
    // durable fallback if the broker is momentarily unavailable.
    public class GlAccountDeactivatedEvent
    {
        public Guid CorrelationId { get; set; }
        public int CompanyId { get; set; }
        public int AccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public int DeactivatedBy { get; set; }
        public DateTimeOffset DeactivatedAt { get; set; }
    }
}
