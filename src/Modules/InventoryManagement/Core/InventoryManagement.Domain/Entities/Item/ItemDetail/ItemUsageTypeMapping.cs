namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemUsageTypeMapping
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int UsageTypeId { get; set; }
        public UsageType UsageType { get; set; } = null!;
        public int UnitId { get; set; }
    }
}
