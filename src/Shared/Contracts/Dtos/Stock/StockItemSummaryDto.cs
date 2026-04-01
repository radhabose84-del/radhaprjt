namespace Contracts.Dtos.Stock
{
    public class StockItemSummaryDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int TotalPackedBags { get; set; }
        public decimal TotalNetWeight { get; set; }
    }
}
