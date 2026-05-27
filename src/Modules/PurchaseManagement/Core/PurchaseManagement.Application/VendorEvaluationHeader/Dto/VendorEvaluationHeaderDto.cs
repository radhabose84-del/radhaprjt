namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorEvaluationHeaderDto
{
    public int Id { get; set; }
    public string? EvaluationCode { get; set; }
    public int VendorId { get; set; }
    public string? VendorName { get; set; }
    public int EvaluationMonth { get; set; }
    public int EvaluationYear { get; set; }
    public DateTimeOffset EvaluationDate { get; set; }
    public decimal TotalWeightedScore { get; set; }
    public int? GradeId { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public int StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }

    // Child details
    public List<VendorEvaluationDetailDto>? VendorEvaluationDetails { get; set; }
}
