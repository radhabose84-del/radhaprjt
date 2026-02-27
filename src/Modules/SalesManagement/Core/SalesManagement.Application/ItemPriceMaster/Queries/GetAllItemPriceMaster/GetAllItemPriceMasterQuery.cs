using Contracts.Common;
using MediatR;
using SalesManagement.Application.ItemPriceMaster.Dto;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetAllItemPriceMaster
{
    public class GetAllItemPriceMasterQuery : IRequest<ApiResponseDTO<List<ItemPriceMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
