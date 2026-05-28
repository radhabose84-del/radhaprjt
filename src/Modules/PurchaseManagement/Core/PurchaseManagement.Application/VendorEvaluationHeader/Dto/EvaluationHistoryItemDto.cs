namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class EvaluationHistoryItemDto
{
    public int EvaluationHeaderId { get; set; }
    public string? EvaluationCode { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public DateTimeOffset EvaluationDate { get; set; }
    public decimal TotalWeightedScore { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public string? EvaluatedBy { get; set; }
    public DateTimeOffset? EvaluatedDate { get; set; }
    public List<CriteriaScoreItemDto>? CriteriaScores { get; set; }
}
