using MediatR;

namespace SalesManagement.Application.SalesContact.Commands.DeleteSalesContact
{
    public sealed record DeleteSalesContactCommand(int Id) : IRequest<bool>;
}
