using MediatR;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment
{
    public class GetPendingSalesOrderAmendmentQuery
        : IRequest<(List<PendingSalesOrderAmendmentDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
