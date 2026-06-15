using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup
{
    public sealed record DeleteCountGroupCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
