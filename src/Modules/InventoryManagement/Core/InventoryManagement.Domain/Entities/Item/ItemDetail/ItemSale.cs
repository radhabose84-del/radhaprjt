namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemSale
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int? UomId { get; set; }
        public UOM? SalesUOM { get; set; }
        public decimal MinQuantity { get; set; }
        public decimal? PackageQuantity { get; set; }
        public int? DeliveryLeadTime { get; set; }
        public bool Discount { get; set; }
        public int? CountId { get; set; }
    }
}
