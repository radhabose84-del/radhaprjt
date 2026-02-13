namespace Contracts.Dtos.Lookups.Warehouse
{
    public sealed class RackLookupDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public string RackCode { get; set; } = string.Empty;
        public string RackName { get; set; } = string.Empty;
    }
}
