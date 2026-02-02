
namespace PurchaseManagement.Domain.Entities
{
    public class ActivityLog
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        public string EntityName { get; set; } = null!;
        public int    EntityId   { get; set; }

        public string Action { get; set; } = "Update";          
        public string PropertyName { get; set; } = default!;    
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public int?    CreatedBy     { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP     { get; set; }

        // Optional scoping helpers
        public string? Scope    { get; set; }   
        public string? ScopeKey { get; set; }   
    }
}
