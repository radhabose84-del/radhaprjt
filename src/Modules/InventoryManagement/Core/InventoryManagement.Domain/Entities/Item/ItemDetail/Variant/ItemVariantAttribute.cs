namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Variant
{
    public class ItemVariantAttribute
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster? ItemMaster { get; set; }
        public int SpecificationMasterId { get; set; }
        public ItemSpecificationMaster? SpecificationMaster { get; set; }
        public int Order { get; set; }
        public ICollection<ItemVariantValue>? ItemVariantValues { get; set; }
    }
}
