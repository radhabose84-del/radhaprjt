using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.VendorEvaluation;

public class VendorEvaluationDetail : BaseEntity
{
    public int VendorEvaluationHeaderId { get; set; }
    public int CriteriaId { get; set; }
    public decimal Score { get; set; }
    public decimal WeightagePercent { get; set; }
    public decimal WeightedScore { get; set; }
    public string? ScoringMethod { get; set; }
    public string? Remarks { get; set; }

    // Same-module navigation
    public VendorEvaluationHeader? VendorEvaluationHeader { get; set; }
    public VendorEvaluationCriteria? VendorEvaluationCriteria { get; set; }
}
