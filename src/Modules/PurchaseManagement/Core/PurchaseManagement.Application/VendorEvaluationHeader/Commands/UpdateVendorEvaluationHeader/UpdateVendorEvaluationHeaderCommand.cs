using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader
{
    public class UpdateVendorEvaluationHeaderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int EvaluationMonth { get; set; }
        public int EvaluationYear { get; set; }
        public DateTimeOffset EvaluationDate { get; set; }
        public decimal TotalWeightedScore { get; set; }
        public int? GradeId { get; set; }
        public int StatusId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<UpdateVendorEvaluationDetailItem>? Details { get; set; }
    }

    public class UpdateVendorEvaluationDetailItem
    {
        public int CriteriaId { get; set; }
        public decimal Score { get; set; }
        public decimal WeightagePercent { get; set; }
        public decimal WeightedScore { get; set; }
        public string? ScoringMethod { get; set; }
        public string? Remarks { get; set; }
    }
}
