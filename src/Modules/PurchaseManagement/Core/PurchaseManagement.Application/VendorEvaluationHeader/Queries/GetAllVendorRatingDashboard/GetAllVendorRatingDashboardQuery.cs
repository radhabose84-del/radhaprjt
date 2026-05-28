using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorRatingDashboard
{
    public class GetAllVendorRatingDashboardQuery : IRequest<ApiResponseDTO<VendorRatingDashboardResponseDto>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? Grade { get; set; }
    }
}
