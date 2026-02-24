#nullable disable
using MediatR;

namespace SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public sealed record DeleteMiscTypeMasterCommand(int Id) : IRequest<bool>;
}
