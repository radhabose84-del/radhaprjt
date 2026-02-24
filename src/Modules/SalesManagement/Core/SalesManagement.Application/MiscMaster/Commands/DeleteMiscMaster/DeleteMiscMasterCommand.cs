#nullable disable
using MediatR;

namespace SalesManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public sealed record DeleteMiscMasterCommand(int Id) : IRequest<bool>;
}
