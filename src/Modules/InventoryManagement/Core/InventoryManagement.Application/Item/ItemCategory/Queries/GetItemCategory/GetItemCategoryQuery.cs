
using InventoryManagement.Application.Common.HttpResponse;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory
{
    public class GetItemCategoryQuery : IRequest<ApiResponseDTO<List<ItemCategoryDto>>>
    {        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}