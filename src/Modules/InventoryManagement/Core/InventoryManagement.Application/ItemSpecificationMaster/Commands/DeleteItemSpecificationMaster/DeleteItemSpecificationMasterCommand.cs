
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster
{
    public sealed record DeleteItemSpecificationMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
