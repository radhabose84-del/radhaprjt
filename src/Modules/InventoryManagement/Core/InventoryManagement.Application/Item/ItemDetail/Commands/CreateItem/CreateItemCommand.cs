using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem
{
    public sealed class CreateItemCommand : IRequest<int>, IRequirePermission
    {
       public ItemDto Payload { get; init; } = default!;
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
