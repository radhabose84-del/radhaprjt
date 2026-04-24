namespace Contracts.Dtos.Stock
{
    public class StockLotByItemDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int? SourceUnitId { get; set; }
        public string? SourceUnitName { get; set; }
    }
}
