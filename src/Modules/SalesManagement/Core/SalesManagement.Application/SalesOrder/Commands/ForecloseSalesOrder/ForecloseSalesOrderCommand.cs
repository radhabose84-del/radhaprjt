using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder
{
    public sealed record ForecloseSalesOrderCommand(int Id) : IRequest<bool>;
}
