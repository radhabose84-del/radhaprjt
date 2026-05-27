namespace Contracts.Dtos.Lookups.Purchase;

public class VendorRatingLookupDto
{
    public int VendorId { get; set; }
    public decimal TotalWeightedScore { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public DateTimeOffset? EvaluationDate { get; set; }
}
