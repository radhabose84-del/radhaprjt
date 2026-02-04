using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem
{
    public sealed class CreateItemCommand : IRequest<int>
    {
       public ItemDto Payload { get; init; } = default!;
    }
}
