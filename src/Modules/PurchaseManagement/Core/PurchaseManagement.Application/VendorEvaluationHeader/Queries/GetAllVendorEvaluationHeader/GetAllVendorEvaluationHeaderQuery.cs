using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorEvaluationHeader
{
    public class GetAllVendorEvaluationHeaderQuery : IRequest<ApiResponseDTO<List<VendorEvaluationHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
