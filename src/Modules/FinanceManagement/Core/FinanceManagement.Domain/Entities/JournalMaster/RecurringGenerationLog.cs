namespace FinanceManagement.Domain.Entities
{
    // US-GL01-11B — one row per template instantiation. UNIQUE(CompanyId, TemplateId, Period) is the
    // idempotency guard; also links each generated journal back to its template. Lean log (NOT a BaseEntity).
    public class RecurringGenerationLog
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }              // cross-module ref -> UserManagement.Company (no DB FK)
        public int TemplateId { get; set; }             // same-module FK -> RecurringJournalTemplateHeader
        public string? Period { get; set; }             // e.g. "2026-06"
        public int? GeneratedVoucherId { get; set; }    // same-module FK -> JournalHeader
        public DateTimeOffset GeneratedAt { get; set; }
        public bool AutoPosted { get; set; }

        // Same-module FK navigation
        public RecurringJournalTemplateHeader? Template { get; set; }
        public JournalHeader? GeneratedVoucher { get; set; }
    }
}
