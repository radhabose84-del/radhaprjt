using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocation
{
    public class GetBarcodeAllocationQuery : IRequest<ApiResponseDTO<List<BarcodeAllocationDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}
