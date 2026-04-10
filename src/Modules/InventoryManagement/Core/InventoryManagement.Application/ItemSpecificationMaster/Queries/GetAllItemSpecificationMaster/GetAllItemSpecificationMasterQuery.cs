
using Contracts.Common;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetAllItemSpecificationMaster
{
    public class GetAllItemSpecificationMasterQuery : IRequest<ApiResponseDTO<List<ItemSpecificationMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
