using MediatR;

namespace SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice
{
    public sealed record DeleteDispatchAdviceCommand(int Id) : IRequest<bool>;
}
