using MediatR;

namespace SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn
{
    public sealed record DeleteSalesReturnCommand(int Id) : IRequest<bool>;
}
