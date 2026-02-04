
namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemLogs
{
    public sealed class ItemLogDto
    {
        public long Id { get; set; }
        public string EntityName { get; set; } = "";
        public int EntityId { get; set; }
        public string Action { get; set; } = "";     
        public string PropertyName { get; set; } = "";
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public DateTimeOffset CreatedDate { get; set; }     
    }
}
