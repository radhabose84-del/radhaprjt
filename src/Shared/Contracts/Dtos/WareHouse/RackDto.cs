namespace Contracts.Dtos.Warehouse
{
    public class RackDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string RackCode { get; set; } = default!;
        public string RackName { get; set; } = default!;
        public int FloorId { get; set; }
        public int AisleId { get; set; }
        public int RackLevelId { get; set; }
        public double MaxCapacity { get; set; }
        public int CapacityUOMId { get; set; }
        public double RackWidth { get; set; }
        public double RackHeight { get; set; }
        public int DimensionUOMId { get; set; }        
    }
}
