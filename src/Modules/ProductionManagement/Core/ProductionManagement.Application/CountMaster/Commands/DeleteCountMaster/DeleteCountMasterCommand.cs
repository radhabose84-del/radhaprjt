using MediatR;

namespace ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster
{
    public sealed record DeleteCountMasterCommand(int Id) : IRequest<bool>;
}
