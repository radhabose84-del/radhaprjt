using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.ProcessMaster.Commands.DeleteProcessMaster
{
    public sealed record DeleteProcessMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
