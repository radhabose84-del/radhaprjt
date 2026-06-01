namespace QCManagement.Domain.Entities
{
    /// <summary>
    /// Field-level audit row (Previous Value → New Value) written by
    /// ActivityLogSaveChangesInterceptor. One row per changed property.
    /// </summary>
    public class ActivityLog
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public string EntityName { get; set; } = string.Empty;
        public int EntityId { get; set; }

        public string Action { get; set; } = "Update";
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }

        public string? Scope { get; set; }
        public string? ScopeKey { get; set; }
    }
}
