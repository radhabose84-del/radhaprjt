using MediatR;

namespace ProductionManagement.Application.RepackingMaster.Commands.DeleteRepackingMaster
{
    public sealed record DeleteRepackingMasterCommand(int Id) : IRequest<bool>;
}
