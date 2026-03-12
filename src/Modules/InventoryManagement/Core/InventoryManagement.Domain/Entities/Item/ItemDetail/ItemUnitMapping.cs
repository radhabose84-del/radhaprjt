namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemUnitMapping
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int ProcurementId { get; set; }
        public ProcurementType ProcurementType { get; set; } = null!;
        public int ItemGroupId { get; set; }
        public ItemGroup ItemGroup { get; set; } = null!;
        public int UnitId { get; set; }
    }
}
