using MediatR;

namespace SalesManagement.Application.LotMaster.Commands.DeleteLotMaster
{
    public sealed record DeleteLotMasterCommand(int Id) : IRequest<bool>;
}
