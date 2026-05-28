using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader
{
    public class CreateVendorEvaluationHeaderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int VendorId { get; set; }
        public int EvaluationMonth { get; set; }
        public int EvaluationYear { get; set; }
        public DateTimeOffset EvaluationDate { get; set; }
        public decimal TotalWeightedScore { get; set; }
        public int? GradeId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateVendorEvaluationDetailItem>? Details { get; set; }
    }

    public class CreateVendorEvaluationDetailItem
    {
        public int CriteriaId { get; set; }
        public decimal Score { get; set; }
        public decimal WeightagePercent { get; set; }
        public decimal WeightedScore { get; set; }
        public string? ScoringMethod { get; set; }
        public string? Remarks { get; set; }
    }
}
