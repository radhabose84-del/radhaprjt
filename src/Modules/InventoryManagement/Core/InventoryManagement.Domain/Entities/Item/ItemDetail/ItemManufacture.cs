namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemManufacture 
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int UnitId { get; set; }
        public int ManufacturingTypeId { get; set; }   
        public MiscMaster MiscManufactureType { get; set; } = null!;
    }
}
