using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemSpecificationMaster : BaseEntity
    {
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
        public ICollection<ItemSpecificationValue> SpecificationValues { get; set; } = new List<ItemSpecificationValue>();
        public ICollection<ItemVariantAttribute> VariantAttributes { get; set; } = new List<ItemVariantAttribute>();
    }
}
