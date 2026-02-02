
namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion
{
    public class QuoteComparisonWorkFlowDto
    {
        public int Id { get; set; }
        public string? RfqCode { get; set; }
        public int RfqId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }        
        //public int RfqHeaderId { get; set; }
        //public int OverrideStatus { get; set; }        

    }
}