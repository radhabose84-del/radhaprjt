namespace FinanceManagement.Domain.Entities
{
    // US-GL02-09 — immutable, field-level statutory change trail for the COA structural masters and their
    // governance entities (IAuditTrailed). Written ONLY by AccountAuditTrailSaveChangesInterceptor, in the
    // same transaction as the change. The table is hardened at the DB level: DENY DELETE/UPDATE to public
    // plus an INSTEAD-OF rollback trigger (AC-2), and is never purged (≥ 8-year retention, AC-5).
    //
    // NOTE: this entity is the audit SINK — it does NOT extend BaseEntity and is NOT IAuditTrailed
    // (it must never audit itself). Mirrors the proven ActivityLog shape + CompanyId + CreatedByRole.
    public class AccountAuditTrail
    {
        public long Id { get; set; }

        public int CompanyId { get; set; }                 // tenant scope + export filter

        public string EntityName { get; set; } = null!;    // e.g. "GlAccountMaster"
        public int EntityId { get; set; }                  // account / group id

        public string Action { get; set; } = "Update";     // "Insert", "Update", "Delete"
        public string PropertyName { get; set; } = default!; // changed field, or "*" for whole-row ops
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int? CreatedBy { get; set; }                // user id
        public string? CreatedByName { get; set; }         // user name
        public string? CreatedByRole { get; set; }         // role active at change time (AC-1)
        public string? CreatedIP { get; set; }
        public DateTimeOffset CreatedDate { get; set; }    // timestamp

        public string? Scope { get; set; }                 // EntityName (filter)
        public string? ScopeKey { get; set; }              // PK value (filter)
    }
}
