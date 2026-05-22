using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster
{
    public sealed record DeleteMiscTypeMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
