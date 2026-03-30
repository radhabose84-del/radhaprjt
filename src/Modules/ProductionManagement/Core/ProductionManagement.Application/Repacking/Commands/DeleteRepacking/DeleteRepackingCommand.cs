using MediatR;

namespace ProductionManagement.Application.Repacking.Commands.DeleteRepacking
{
    public sealed record DeleteRepackingCommand(int Id) : IRequest<bool>;
}
