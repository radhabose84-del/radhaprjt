using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems
{
    public sealed class GetAllItemsQuery
        : IRequest<(List<ItemListDto> Items, int TotalCount)>   // ✅ match handler
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? SearchTerm { get; init; }
        public bool OnlyActive { get; init; } = true;
        
        public int? ItemGroupId   { get; set; }
        public int? ItemCategoryId { get; set; }
    }
}
