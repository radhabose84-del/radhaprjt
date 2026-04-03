namespace Contracts.Dtos.Stock
{
    public class StockItemSummaryDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
    }
}
