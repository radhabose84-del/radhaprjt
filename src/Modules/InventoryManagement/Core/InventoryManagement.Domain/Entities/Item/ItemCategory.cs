using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.PutAway;

namespace InventoryManagement.Domain.Entities.Item
{
    public class ItemCategory : BaseEntity
    {
        public int ItemGroupId { get; set; }
        public ItemGroup ItemGroup { get; set; } = null!;
        public string? ItemCategoryName { get; set; }
        public byte? IsGroup { get; set; }
        public int? ParentCategoryId { get; set; }
        public ItemCategory ItemCategoryParent { get; set; } = null!;
        public ICollection<ItemCategory>? ChildCategories { get; set; } = new List<ItemCategory>(); // For hierarchical categories
        public byte? IsBudgetApplicable { get; set; }
        public int? EmergencyPOById { get; set; }
        public decimal? EmergencyValueLimit { get; set; }
        public int? EmergencyActionId { get; set; }
        public int? RootCategoryId { get; set; }
        public ItemCategory? RootCategory { get; set; }
        public int? DeptId { get; set; }
        public ICollection<ItemMaster>? ItemMasterCategory { get; set; }
        public ICollection<PutAwayRule>? PutAwayRuleCategory { get; set; } = new List<PutAwayRule>();
        public ICollection<ItemCategoryModule> ItemCategoryModules { get; set; } = new List<ItemCategoryModule>();
        public ICollection<ItemCategoryUnitConfig> UnitConfigs { get; set; } = new List<ItemCategoryUnitConfig>();
    }
}