using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetAllQuotations
{
    public class GetAllQuotationsQuery 
        : IRequest<(IReadOnlyList<QuotationListItemDto> Items, int Total)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize  { get; set; } = 20;
        public string? SearchTerm { get; set; }
    }
}
