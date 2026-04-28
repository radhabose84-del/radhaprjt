using MediatR;

namespace SalesManagement.Application.SalesQuotation.Queries.GetPendingSalesQuotationAmendment
{
    public class GetPendingSalesQuotationAmendmentQuery
        : IRequest<(List<PendingSalesQuotationAmendmentDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
