using Contracts.Common;
using MediatR;
using InventoryManagement.Application.UsageType.Dto;

namespace InventoryManagement.Application.UsageType.Queries.GetAllUsageType
{
    public class GetAllUsageTypeQuery : IRequest<ApiResponseDTO<List<UsageTypeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
