using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate
{
    public sealed class CreateItemTemplateCommand : IRequest<int>, IRequirePermission
    {
        public ItemDto Payload { get; init; } = default!;
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
