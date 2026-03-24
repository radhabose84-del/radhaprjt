using MediatR;

namespace GateEntryManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public sealed record DeleteMiscTypeMasterCommand(int Id) : IRequest<bool>;
}
