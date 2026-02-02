namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById
{
    public class QuoteCompareByIdDto
    {
        public int Id { get; set; }
        public int RfqId { get; set; }
        public string? RfqCode { get; set; }
        public int StatusId { get; set; }
    }
}