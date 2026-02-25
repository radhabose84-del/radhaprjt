using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesItemPriceMaster.Dto;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetAllSalesItemPriceMaster
{
    public class GetAllSalesItemPriceMasterQuery : IRequest<ApiResponseDTO<List<SalesItemPriceMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
