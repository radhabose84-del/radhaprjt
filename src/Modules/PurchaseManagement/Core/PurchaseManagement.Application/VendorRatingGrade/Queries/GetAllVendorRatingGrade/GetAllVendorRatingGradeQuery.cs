using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.VendorRatingGrade.Dto;

namespace PurchaseManagement.Application.VendorRatingGrade.Queries.GetAllVendorRatingGrade
{
    public class GetAllVendorRatingGradeQuery : IRequest<ApiResponseDTO<List<VendorRatingGradeDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
