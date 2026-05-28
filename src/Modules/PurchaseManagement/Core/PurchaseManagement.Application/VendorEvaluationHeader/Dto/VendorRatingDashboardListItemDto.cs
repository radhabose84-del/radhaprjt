namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorRatingDashboardListItemDto
{
    public int VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public decimal LatestScore { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public string? Action { get; set; }
    public string? Trend { get; set; }
    public decimal? TrendPercentage { get; set; }
    public DateTimeOffset? LastEvaluatedDate { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public int LatestEvaluationHeaderId { get; set; }
    public List<CriteriaScoreItemDto>? CriteriaScores { get; set; }
}
