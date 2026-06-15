using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants
{
    public sealed class CreateItemVariantsCommand : IRequest<List<int>>, IRequirePermission
    {
        public ItemDto Payload { get; init; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
