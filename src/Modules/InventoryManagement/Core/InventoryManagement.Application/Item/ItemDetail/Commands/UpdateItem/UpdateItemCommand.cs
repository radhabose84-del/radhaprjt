
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem
{
    public sealed class UpdateItemCommand :  IRequest<Unit>, IRequirePermission
    {
        //public int Id { get; init; }                        
        public ItemDto Payload { get; init; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
