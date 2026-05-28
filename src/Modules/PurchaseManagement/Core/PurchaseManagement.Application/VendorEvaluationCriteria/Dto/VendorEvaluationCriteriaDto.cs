namespace PurchaseManagement.Application.VendorEvaluationCriteria.Dto;

public class VendorEvaluationCriteriaDto
{
    public int Id { get; set; }
    public string? CriteriaCode { get; set; }
    public string? CriteriaName { get; set; }
    public string? Description { get; set; }
    public decimal WeightagePercent { get; set; }
    public int ScoringMethodId { get; set; }
    public string? ScoringMethodName { get; set; }
    public decimal MinimumScore { get; set; }
    public int RatingImpactId { get; set; }
    public string? RatingImpactName { get; set; }
    public int SortOrder { get; set; }
    public string? CalculationType { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
}
