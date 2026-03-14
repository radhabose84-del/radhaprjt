using MediatR;

namespace ProductionManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public sealed record DeleteMiscMasterCommand(int Id) : IRequest<bool>;
}
