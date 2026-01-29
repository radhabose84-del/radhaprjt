namespace Contracts.Dtos.Warehouse
{
    public sealed class BinDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int? RackId { get; set; }
        public string BinCode { get; set; } = "";
        public string BinName { get; set; } = "";
        public double BinCapacity { get; set; }
        public string? WarehouseCode { get; set; } = "";
        public string? WarehouseName { get; set; } = "";
        public string? RackCode { get; set; } = "";
        public string? RackName { get; set; } = "";        
        public int CapacityUOMId { get; set; }        
    }
}
