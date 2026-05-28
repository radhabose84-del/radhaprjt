using MediatR;

namespace QCManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public sealed record DeleteMiscTypeMasterCommand(int Id) : IRequest<bool>;
}
