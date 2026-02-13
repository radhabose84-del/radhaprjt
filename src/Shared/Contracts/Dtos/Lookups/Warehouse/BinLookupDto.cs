namespace Contracts.Dtos.Lookups.Warehouse
{
    public sealed class BinLookupDto
    {
        public int Id { get; set; }
        public int WarehouseId { get; set; }
        public int? RackId { get; set; }
        public string BinCode { get; set; } = string.Empty;
        public string BinName { get; set; } = string.Empty;
    }
}
