using MediatR;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById
{
    public class GetPendingSalesOrderByIdQuery : IRequest<PendingSalesOrderByIdDto?>
    {
        public int Id { get; set; }
    }
}
