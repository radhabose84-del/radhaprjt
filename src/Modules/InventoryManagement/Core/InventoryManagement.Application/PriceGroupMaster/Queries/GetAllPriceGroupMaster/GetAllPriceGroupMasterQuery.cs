using Contracts.Common;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Queries.GetAllPriceGroupMaster
{
    public class GetAllPriceGroupMasterQuery : IRequest<ApiResponseDTO<List<PriceGroupMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
