using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig
{
    public sealed record DeleteMovementTypeConfigCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
