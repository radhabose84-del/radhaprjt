using MediatR;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrder
{
    public sealed class GetPendingSalesOrderQuery
        : IRequest<(List<PendingSalesOrderDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
