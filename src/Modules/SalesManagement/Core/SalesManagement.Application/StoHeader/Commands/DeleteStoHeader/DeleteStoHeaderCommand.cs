using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoHeader.Commands.DeleteStoHeader
{
    public sealed record DeleteStoHeaderCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
