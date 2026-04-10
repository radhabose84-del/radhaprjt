using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemItemSpecification : BaseEntity
    {
        public int ItemId { get; set; }
        public ItemMaster? ItemMaster { get; set; }
        public int SpecificationValueId { get; set; }
        public ItemSpecificationValue? SpecificationValue { get; set; }
    }
}
