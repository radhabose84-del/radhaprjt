using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item
{
    public class ItemCategoryUnitConfig : BaseEntity
    {
        public int ItemCategoryId { get; set; }
        public ItemCategory? ItemCategory { get; set; }

        public int UnitId { get; set; }

        public int UOMId { get; set; }
        public UOM? UOM { get; set; }

        public decimal MaxSampleQuantity { get; set; }
    }
}
