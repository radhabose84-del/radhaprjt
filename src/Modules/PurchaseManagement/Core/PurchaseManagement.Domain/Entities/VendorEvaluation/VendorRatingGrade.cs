using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.VendorEvaluation;

public class VendorRatingGrade : BaseEntity
{
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public decimal MinScore { get; set; }
    public decimal MaxScore { get; set; }
    public string? ActionDescription { get; set; }
    public int? ActionTypeId { get; set; }
    public int SortOrder { get; set; }

    // Same-module navigation — MiscMaster
    public MiscMaster? ActionType { get; set; }

    // Reverse navigation
    public ICollection<VendorEvaluationHeader>? VendorEvaluationHeaders { get; set; }
}
