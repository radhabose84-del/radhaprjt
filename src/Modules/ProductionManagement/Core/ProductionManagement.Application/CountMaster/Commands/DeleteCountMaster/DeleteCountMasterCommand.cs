using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster
{
    public sealed record DeleteCountMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
