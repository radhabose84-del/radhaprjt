using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Domain.Entities.Item.PutAway;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemMaster : BaseEntity
    {
        public string ItemCode { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public int? HSNId { get; set; }
        public HSNMaster HSNMaster { get; set; } = null!;
        public int? ItemGroupId { get; set; }
        public ItemGroup ItemGroup { get; set; } = null!;
        public int? ItemCategoryId { get; set; }
        public ItemCategory ItemCategory { get; set; } = null!;
        public int? StockUomId { get; set; }
        public UOM UOM { get; set; } = null!;
        public int? ItemClassificationId { get; set; }
        public MiscMaster MiscClassification { get; set; } = null!;
        public string? Description { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public int? XPlantMaterialStatusId { get; set; }
        public MiscMaster MiscStatus { get; set; } = null!;
        //public int? DepartmentId { get; set; }
        public bool IsStockItem { get; set; }        
        public bool MaintainStock { get; set; }
        public bool HasVariants { get; set; }
        public bool IsCapitalItem { get; set; }
        public int? ParentItemId { get; set; }
        public ItemMaster? ParentItem { get; set; }
        public ICollection<ItemMaster> ChildItems { get; set; } = new List<ItemMaster>();
        public string? ItemImage { get; set; }
        public ItemPurchase? Purchase { get; set; }
        public ItemInventory? Inventory { get; set; }
        public ItemQuality? Quality { get; set; }
        public ItemSale? Sale { get; set; }
        public int? IssueRuleId { get; set; }
        public MiscMaster? MiscIssueRule { get; set; } = null!;
        public int? OriginCountryId { get; set; }
        public string? TariffNumber { get; set; }
        public bool IsOnSpot { get; set; }=false;
        public ICollection<ItemVariantValue> VariantValues { get; set; } = new List<ItemVariantValue>();
        public ICollection<ItemSupplier> Suppliers { get; set; } = new List<ItemSupplier>();
        public ICollection<ItemManufacture> Manufacture { get; set; } = new List<ItemManufacture>();
        public ICollection<ItemUOM> ItemUOMs { get; set; } = new List<ItemUOM>();
        public ICollection<PutAwayRule>? PutAwayRules { get; set; } = new List<PutAwayRule>();
        public ICollection<ItemVariantAttribute> VariantAttributes { get; set; } = new List<ItemVariantAttribute>();
        public ICollection<ItemVariantValue> VariantParentItem { get; set; } = new List<ItemVariantValue>();
        public ICollection<ItemUsageTypeMapping> ItemUsageTypeMappings { get; set; } = new List<ItemUsageTypeMapping>();
    }
}