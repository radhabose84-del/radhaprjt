using Contracts.Common;
using MediatR;
using SalesManagement.Application.DiscountMaster.Dto;

namespace SalesManagement.Application.DiscountMaster.Queries.GetAllDiscountMaster
{
    public class GetAllDiscountMasterQuery : IRequest<ApiResponseDTO<List<DiscountMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
