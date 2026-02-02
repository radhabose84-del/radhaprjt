
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparision
{
    public class CreateQuoteComparisonReverseDto
    {
        public QuoteComparisonWorkFlowDto? Header { get; set; }
        public ICollection<QuoteComparisonWorkFlowDto>? Lines { get; set; }
    }
    
}