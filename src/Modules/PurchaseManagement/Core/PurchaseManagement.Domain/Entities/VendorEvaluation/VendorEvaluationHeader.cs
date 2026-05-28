using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.VendorEvaluation;

public class VendorEvaluationHeader : BaseEntity
{
    public string? EvaluationCode { get; set; }
    public int VendorId { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public DateTimeOffset EvaluationDate { get; set; }
    public decimal TotalWeightedScore { get; set; }
    public int? GradeId { get; set; }
    public string? Remarks { get; set; }

    // Same-module navigation
    public VendorRatingGrade? Grade { get; set; }

    // Child collection
    public ICollection<VendorEvaluationDetail>? VendorEvaluationDetails { get; set; }
}
