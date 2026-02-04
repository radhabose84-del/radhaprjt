namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemVariantAttribute
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster ItemMaster { get; set; } = null!;
        public int VariantBasedOn { get; set; }
        public MiscMaster MiscVariantBasedOn { get; set; } = null!;
        public int? AttributeGroupId { get; set; }
        public MiscTypeMaster? MiscAttributeGroup { get; set; }
        public int AttributeId { get; set; }
        public MiscMaster MiscAttribute { get; set; } = null!;
        public int Order { get; set; }   
        public ICollection<ItemVariantValue>? ItemVariantValues { get; set; }
    }
}
