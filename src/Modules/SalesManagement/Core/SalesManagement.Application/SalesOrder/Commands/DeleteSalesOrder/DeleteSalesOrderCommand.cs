using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrder
{
    public sealed record DeleteSalesOrderCommand(int Id) : IRequest<bool>;
}
