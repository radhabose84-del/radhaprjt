namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemVariantValue
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public virtual ItemMaster? ItemMaster { get; set; }
        public int VariantAttributeId { get; set; }
        public ItemVariantAttribute? VariantAttribute { get; set; }
        public int SpecificationValueId { get; set; }
        public ItemSpecificationValue? SpecificationValue { get; set; }
        public int ParentItemId { get; set; }
        public ItemMaster? ParentItem { get; set; }
    }
}
