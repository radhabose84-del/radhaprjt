using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade
{
    public class UpdateVendorRatingGradeCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string? GradeName { get; set; }
        public decimal MinScore { get; set; }
        public decimal MaxScore { get; set; }
        public string? ActionDescription { get; set; }
        public int? ActionTypeId { get; set; }
        public int SortOrder { get; set; }
        public int IsActive { get; set; }
    }
}
