using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.CancelSalesOrder
{
    public sealed record CancelSalesOrderCommand(int Id) : IRequest<bool>;
}
