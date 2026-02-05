
namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemUOM
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster? Item { get; set; } 
        public int? ConversionUOMId { get; set; }
        public UOM? ConversionUOM { get; set; }
        public decimal? ConversionRate { get; set; }
    }
}