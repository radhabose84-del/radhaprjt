
namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemPurchase
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int? PurchaseUomId { get; set; }
        public UOM PurchaseUOM { get; set; } = null!;
        public int? LeadTimeDays { get; set; }
        public int? SafetyStock { get; set; }
        public int? GrProcessingTimeDays { get; set; }
        public decimal? PurchaseRate { get; set; }
        public bool AutomaticPo { get; set; }
        public int? OriginCountryId { get; set; }
        public string? TariffNumber { get; set; }
        public int? SourceOfItem { get; set; }
        public MiscMaster MiscSource { get; set; } = null!;
        
    }
}
