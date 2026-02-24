#nullable disable
using MediatR;

namespace SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup
{
    public sealed record DeleteSalesGroupCommand(int Id) : IRequest<bool>;
}
