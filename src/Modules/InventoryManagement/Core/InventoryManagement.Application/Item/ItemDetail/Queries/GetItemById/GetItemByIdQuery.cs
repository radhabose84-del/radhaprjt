using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemById
{
    public sealed class GetItemByIdQuery : IRequest<ItemDetailsDto?>
    {
        public int Id { get; init; }
    }
}
