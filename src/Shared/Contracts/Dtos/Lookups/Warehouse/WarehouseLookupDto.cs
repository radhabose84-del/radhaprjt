namespace Contracts.Dtos.Lookups.Warehouse
{
    public sealed class WarehouseLookupDto
    {
        public int Id { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int? ParentWarehouseId { get; set; }
    }
}
