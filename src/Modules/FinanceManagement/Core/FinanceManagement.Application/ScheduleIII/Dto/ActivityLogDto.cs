namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ActivityLogDto
    {
        public long Id { get; set; }
        public string? EntityName { get; set; }
        public int EntityId { get; set; }
        public string? Action { get; set; }          // "Update" / "Delete"
        public string? PropertyName { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? Scope { get; set; }
        public string? ScopeKey { get; set; }
    }
}
