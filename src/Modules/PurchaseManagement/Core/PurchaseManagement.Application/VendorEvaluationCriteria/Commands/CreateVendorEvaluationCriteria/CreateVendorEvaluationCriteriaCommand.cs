using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria
{
    public class CreateVendorEvaluationCriteriaCommand : IRequest<ApiResponseDTO<int>>
    {
        public string? CriteriaCode { get; set; }
        public string? CriteriaName { get; set; }
        public string? Description { get; set; }
        public decimal WeightagePercent { get; set; }
        public int ScoringMethodId { get; set; }
        public decimal MinimumScore { get; set; }
        public int RatingImpactId { get; set; }
        public int SortOrder { get; set; }
    }
}
