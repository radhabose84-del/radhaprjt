using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderById
{
    public class GetSalesOrderByIdQuery : IRequest<SalesOrderHeaderDto?>
    {
        public int Id { get; set; }
    }
}
