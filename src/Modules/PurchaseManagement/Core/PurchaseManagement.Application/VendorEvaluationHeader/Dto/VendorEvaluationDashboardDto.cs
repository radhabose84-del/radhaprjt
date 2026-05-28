namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorEvaluationDashboardDto
{
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public int LookbackMonths { get; set; }
    public List<DashboardCriteriaDto>? Criteria { get; set; }
    public decimal TotalWeightedScore { get; set; }
    public int? ResolvedGradeId { get; set; }
    public string? ResolvedGradeCode { get; set; }
    public string? ResolvedGradeName { get; set; }
    public List<GradeReferenceDto>? GradeReferences { get; set; }
}
