namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class DashboardCriteriaDto
{
    public int CriteriaId { get; set; }
    public string? CriteriaCode { get; set; }
    public string? CriteriaName { get; set; }
    public string? Description { get; set; }
    public decimal WeightagePercent { get; set; }
    public string? ScoringMethodName { get; set; }
    public string? CalculationType { get; set; }
    public bool IsAutoCalculated { get; set; }
    public decimal? AutoCalculatedScore { get; set; }
    public decimal MinimumScore { get; set; }
}
