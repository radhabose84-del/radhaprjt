using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster
{
    public sealed record DeleteYarnTwistMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
