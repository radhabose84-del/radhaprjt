namespace InventoryManagement.Application.Item.ItemCategory.Queries.Shared
{
    public sealed class SampleQuantityDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int UOMId { get; set; }
        public string? UOMName { get; set; }
        public decimal MaxSampleQuantity { get; set; }
        public int IsActive { get; set; }
    }
}
