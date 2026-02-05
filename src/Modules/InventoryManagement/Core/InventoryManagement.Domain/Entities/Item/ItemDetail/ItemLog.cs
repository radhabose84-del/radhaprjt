namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemLog
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string EntityName { get; set; } = null!;
        public int EntityId { get; set; }
        public string Action { get; set; } = "Update";
        public string PropertyName { get; set; } = default!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public string? CorrelationId { get; set; }
    }
}