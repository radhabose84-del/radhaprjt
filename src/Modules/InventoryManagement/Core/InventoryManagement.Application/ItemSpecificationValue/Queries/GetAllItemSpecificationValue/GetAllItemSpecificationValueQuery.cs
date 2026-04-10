
using Contracts.Common;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetAllItemSpecificationValue
{
    public class GetAllItemSpecificationValueQuery : IRequest<ApiResponseDTO<List<ItemSpecificationValueDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
