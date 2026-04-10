using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Domain.Entities.MRS;

namespace InventoryManagement.Domain.Entities
{

    public class MiscMaster : BaseEntity
    {
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }

        public int SortOrder { get; set; }
        public MiscTypeMaster? MiscTypeMaster { get; set; }
        public ICollection<BudgetLog>? BudgetAction { get; set; }
        public ICollection<HSNMaster>? HSNMasters { get; set; }
        public ICollection<HSNMaster>? TypeHSNs { get; set; }
        public ICollection<UOM> UOMs { get; set; } = new List<UOM>();
        public ICollection<ItemMaster>? ItemMasterStatus { get; set; }
        public ICollection<ItemMaster>? ItemMasterClassification { get; set; }
        public ICollection<ItemPurchase>? ItemPurchaseSource { get; set; }
        public ICollection<ItemInventory>? ItemInventoryRequestType { get; set; }
        public ICollection<ItemInventory>? ItemInventoryValuationMethod { get; set; }
        public ICollection<ItemInventory>? ItemInventoryDefaultMaterialRequestType { get; set; }
        public ICollection<ItemSale>? ItemSaleValuationMethod { get; set; }
        public ICollection<ItemManufacture>? ItemManufactureType { get; set; }
        public ICollection<ItemQuality>? ItemQualityCertificateType { get; set; }
        public ICollection<PutAwayStrategy>? PutAwayStrategyStorageType { get; set; }
        public ICollection<PutAwayStrategy>? PutAwayStrategyPriority { get; set; }
        public ICollection<ItemMaster>? ItemMasterIssueRule { get; set; }  
        public ICollection<MrsHeader>? MrsDetailsHeader { get; set; }
        public ICollection<MrsHeader>? MrsRequestHeader { get; set; }
   
    }
}