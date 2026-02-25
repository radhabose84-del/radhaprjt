using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison
{
    public class GetQuoteComparisonQuery : IRequest<QuoteComparisonDto?>
    {
        public int RfqId { get; set; }

        public GetQuoteComparisonQuery(int rfqId)
        {
            RfqId = rfqId;
        }
    }
}