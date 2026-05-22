using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.LotMaster.Commands.DeleteLotMaster
{
    public sealed record DeleteLotMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
