namespace FinanceManagement.Domain.Entities
{
    // US-GL01-16B — one row per threshold breach on a posted journal. Drives the flagged-for-review
    // list and the daily auditor digest. Alert-only. Lean log (NOT a BaseEntity).
    public class JournalFlag
    {
        public int Id { get; set; }

        public int JournalHeaderId { get; set; }        // same-module FK -> JournalHeader
        public int RuleTypeId { get; set; }             // same-module FK -> MiscMaster (THRESHOLD_RULE_TYPE)
        public decimal? Value { get; set; }             // the breaching value
        public DateTimeOffset FlaggedAt { get; set; }
        public bool DigestSent { get; set; }

        // Same-module FK navigation
        public JournalHeader? JournalHeader { get; set; }
        public MiscMaster? RuleType { get; set; }
    }
}
