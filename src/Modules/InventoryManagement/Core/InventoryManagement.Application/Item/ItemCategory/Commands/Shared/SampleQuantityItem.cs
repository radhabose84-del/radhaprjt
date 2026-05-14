namespace InventoryManagement.Application.Item.ItemCategory.Commands.Shared
{
    public sealed class SampleQuantityItem
    {
        public int? Id { get; set; }
        public int UnitId { get; set; }
        public int UOMId { get; set; }
        public decimal MaxSampleQuantity { get; set; }
        public byte IsActive { get; set; }
    }
}
