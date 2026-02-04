using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemTemplate
{
    public sealed class CreateItemTemplateCommand : IRequest<int>
    {
        public ItemDto Payload { get; init; } = default!;
    }
}