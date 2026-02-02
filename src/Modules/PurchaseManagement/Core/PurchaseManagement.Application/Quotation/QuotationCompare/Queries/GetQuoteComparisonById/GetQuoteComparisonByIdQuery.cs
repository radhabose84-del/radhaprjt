using MediatR;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById
{
    public class GetQuoteComparisonByIdQuery : IRequest<QuoteCompareByIdDto>
    {
        public int Id { get; set; }
    }
}