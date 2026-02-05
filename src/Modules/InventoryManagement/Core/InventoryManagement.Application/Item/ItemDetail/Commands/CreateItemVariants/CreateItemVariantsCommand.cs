using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants
{
    public sealed class CreateItemVariantsCommand : IRequest<List<int>>
    {
        public ItemDto Payload { get; init; } = default!;
    }
}
