using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster;

public sealed record DeleteDispatchAddressMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
