
using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue
{
    public sealed record DeleteItemSpecificationValueCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
