using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetAllVendorEvaluationCriteria
{
    public class GetAllVendorEvaluationCriteriaQuery : IRequest<ApiResponseDTO<List<VendorEvaluationCriteriaDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
