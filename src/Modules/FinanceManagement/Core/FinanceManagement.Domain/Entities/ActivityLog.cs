namespace FinanceManagement.Domain.Entities
{
    // Relational, property-level change trail for IActivityTracked entities (Finance.ActivityLog).
    public class ActivityLog
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public string EntityName { get; set; } = null!;
        public int EntityId { get; set; }

        public string Action { get; set; } = "Update";        // "Insert", "Update", "Delete"
        public string PropertyName { get; set; } = default!;   // changed property, or "*" for bulk ops
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }

        public string? Scope { get; set; }     // EntityName (for filtering)
        public string? ScopeKey { get; set; }   // PK value (for filtering)
    }
}
