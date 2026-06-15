
using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.BusinessUnit.Commands.DeleteBusinessUnit
{
    public sealed record DeleteBusinessUnitCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
