using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupById
{
    public class GetItemGroupByIdQuery : IRequest<ItemGroupDto>
    {
        public int Id { get; set; }
    }
}