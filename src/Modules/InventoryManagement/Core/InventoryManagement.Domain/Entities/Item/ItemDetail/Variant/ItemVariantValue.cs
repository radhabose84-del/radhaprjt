namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemVariantValue
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public virtual ItemMaster ItemMaster { get; set; } = default!;
        public int VariantAttributeId { get; set; }
        public ItemVariantAttribute VariantAttribute { get; set; } = null!;
        public string OptionValue { get; set; } = null!;
        public int ParentItemId { get; set; } 
        public ItemMaster ParentItem { get; set; } = null!;
    }
}
