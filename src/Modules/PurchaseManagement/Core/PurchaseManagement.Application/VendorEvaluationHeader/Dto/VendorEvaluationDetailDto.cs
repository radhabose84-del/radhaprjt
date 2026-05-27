namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorEvaluationDetailDto
{
    public int Id { get; set; }
    public int VendorEvaluationHeaderId { get; set; }
    public int CriteriaId { get; set; }
    public string? CriteriaCode { get; set; }
    public string? CriteriaName { get; set; }
    public decimal Score { get; set; }
    public decimal WeightagePercent { get; set; }
    public decimal WeightedScore { get; set; }
    public string? ScoringMethod { get; set; }
    public string? Remarks { get; set; }
}
