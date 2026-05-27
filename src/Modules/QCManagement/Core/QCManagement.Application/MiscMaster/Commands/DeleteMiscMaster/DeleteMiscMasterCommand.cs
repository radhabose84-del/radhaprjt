using MediatR;

namespace QCManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public sealed record DeleteMiscMasterCommand(int Id) : IRequest<bool>;
}
