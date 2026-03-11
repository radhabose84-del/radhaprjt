using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Domain.Entities
{
    public class UOM : BaseEntity
    {
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int UOMTypeId { get; set; } // Foreign Key to MiscMaster
        public int SortOrder { get; set; }

        // Foreign Key relationship with MiscMaster
        public MiscMaster UOMType { get; set; } = null!;

        public ICollection<UOMConversion>? FromUOMConversions { get; set; }
        public ICollection<UOMConversion>? ToUOMConversions { get; set; }
        public ICollection<ItemMaster>? ItemMasterUOM { get; set; }
        public ICollection<ItemUOM>? ItemUOM { get; set; }
        public ICollection<ItemPurchase>? PurchaseUOM { get; set; }
        public ICollection<ItemInventory>? InventoryUOM { get; set; }
        public ICollection<ItemSale>? SalesUOM { get; set; }

    }
}