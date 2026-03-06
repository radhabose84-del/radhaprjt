using MediatR;

namespace SalesManagement.Application.StoHeader.Commands.DeleteStoHeader
{
    public sealed record DeleteStoHeaderCommand(int Id) : IRequest<bool>;
}
