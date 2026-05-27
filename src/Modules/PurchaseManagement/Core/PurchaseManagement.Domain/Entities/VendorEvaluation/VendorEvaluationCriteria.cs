using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.VendorEvaluation;

public class VendorEvaluationCriteria : BaseEntity
{
    public string? CriteriaCode { get; set; }
    public string? CriteriaName { get; set; }
    public string? Description { get; set; }
    public decimal WeightagePercent { get; set; }
    public int ScoringMethodId { get; set; }
    public decimal MinimumScore { get; set; }
    public int RatingImpactId { get; set; }
    public int SortOrder { get; set; }

    // Same-module navigation — MiscMaster
    public MiscMaster? ScoringMethod { get; set; }
    public MiscMaster? RatingImpact { get; set; }

    // Reverse navigation
    public ICollection<VendorEvaluationDetail>? VendorEvaluationDetails { get; set; }
}
