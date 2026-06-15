using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster
{
    public sealed record DeletePriceGroupMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
