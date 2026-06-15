using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.StoTypeMaster.Commands.DeleteStoTypeMaster
{
    public sealed record DeleteStoTypeMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
