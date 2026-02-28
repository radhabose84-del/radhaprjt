using MediatR;

namespace SalesManagement.Application.CustomerVisit.Commands.DeleteCustomerVisit
{
    public sealed record DeleteCustomerVisitCommand(int Id) : IRequest<bool>;
}
