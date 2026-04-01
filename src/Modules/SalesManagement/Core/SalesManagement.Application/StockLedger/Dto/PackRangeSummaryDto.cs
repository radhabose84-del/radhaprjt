namespace SalesManagement.Application.StockLedger.Dto
{
    public class PackRangeSummaryDto
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int TotalPacks { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }
}
