using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.PutAway;

namespace InventoryManagement.Domain.Entities.Item
{
    public class ItemGroup : BaseEntity
    {
        public int UnitId { get; set; }
        public string? ItemGroupCode { get; set; }
        public string? ItemGroupName { get; set; }
        public ICollection<ItemCategory>? ItemCategory { get; set; }
        public ICollection<ItemMaster>? ItemMasterGroup { get; set; } 
        public ICollection<PutAwayRule>? PutAwayRuleGroup { get; set; } = new List<PutAwayRule>();
        public ICollection<ItemUnitMapping>? ItemUnitMappings { get; set; }
    }
}