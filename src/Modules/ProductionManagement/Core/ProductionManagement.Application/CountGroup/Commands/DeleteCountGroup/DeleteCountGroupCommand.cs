using MediatR;

namespace ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup
{
    public sealed record DeleteCountGroupCommand(int Id) : IRequest<bool>;
}
