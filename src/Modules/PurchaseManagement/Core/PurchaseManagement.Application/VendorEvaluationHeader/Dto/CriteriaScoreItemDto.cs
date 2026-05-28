namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class CriteriaScoreItemDto
{
    public int CriteriaId { get; set; }
    public string? CriteriaCode { get; set; }
    public string? CriteriaName { get; set; }
    public decimal Score { get; set; }
    public decimal WeightagePercent { get; set; }
    public decimal WeightedScore { get; set; }
}
