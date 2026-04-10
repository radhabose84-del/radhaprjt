using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemSpecificationValue : BaseEntity
    {
        public int SpecificationMasterId { get; set; }
        public ItemSpecificationMaster? SpecificationMaster { get; set; }
        public string? SpecificationValue { get; set; }
        public ICollection<ItemVariantValue> VariantValues { get; set; } = new List<ItemVariantValue>();
        public ICollection<ItemItemSpecification> ItemSpecifications { get; set; } = new List<ItemItemSpecification>();
    }
}
