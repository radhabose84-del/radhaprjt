using MediatR;

namespace ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader
{
    public sealed record DeleteRepackingHeaderCommand(int Id) : IRequest<bool>;
}
