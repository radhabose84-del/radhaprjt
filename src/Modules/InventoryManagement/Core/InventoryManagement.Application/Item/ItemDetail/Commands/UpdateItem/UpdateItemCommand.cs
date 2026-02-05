
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem
{
    public sealed class UpdateItemCommand :  IRequest<Unit>
    {
        //public int Id { get; init; }                        
        public ItemDto Payload { get; init; } = default!;
    }
}
