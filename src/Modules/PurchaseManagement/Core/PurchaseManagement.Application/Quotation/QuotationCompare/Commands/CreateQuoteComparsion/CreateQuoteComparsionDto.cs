namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
{
    public class CreateQuoteComparsionDto
    {
        public int RfqId { get; set; }
        public string? RfqCode { get; set; }
        public List<CreateQuoteComparsionDetailDto> Details { get; set; } = new();
        public class CreateQuoteComparsionDetailDto
        {
            public int QuotationHeaderId { get; set; }
            public int QuotationDetailId { get; set; }
            public decimal Net { get; set; }
            public decimal LandedUnit { get; set; }
            public decimal Total { get; set; }
            public byte OverrideStatus { get; set; }
            public string? Remarks { get; set; }
        }
    }
}