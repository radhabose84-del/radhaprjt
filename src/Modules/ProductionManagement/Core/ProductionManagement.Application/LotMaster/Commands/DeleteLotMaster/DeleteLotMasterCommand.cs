using MediatR;

namespace ProductionManagement.Application.LotMaster.Commands.DeleteLotMaster
{
    public sealed record DeleteLotMasterCommand(int Id) : IRequest<bool>;
}
