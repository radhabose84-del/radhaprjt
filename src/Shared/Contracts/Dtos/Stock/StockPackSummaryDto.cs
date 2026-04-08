namespace Contracts.Dtos.Stock
{
    public class StockPackSummaryDto
    {
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public string? BatchNumber { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeCode { get; set; }
        public string? PackTypeName { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public decimal TareWeight { get; set; }
        public decimal GrossWeight { get; set; }
        public int ConesPerBag { get; set; }
    }
}
