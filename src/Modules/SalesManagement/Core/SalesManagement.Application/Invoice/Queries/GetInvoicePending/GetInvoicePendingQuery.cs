using MediatR;

namespace SalesManagement.Application.Invoice.Queries.GetInvoicePending
{
    public sealed class GetInvoicePendingQuery
        : IRequest<(List<GetInvoicePendingDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
