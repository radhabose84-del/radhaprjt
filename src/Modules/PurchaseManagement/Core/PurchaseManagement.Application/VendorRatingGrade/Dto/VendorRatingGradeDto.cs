namespace PurchaseManagement.Application.VendorRatingGrade.Dto;

public class VendorRatingGradeDto
{
    public int Id { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public decimal MinScore { get; set; }
    public decimal MaxScore { get; set; }
    public string? ActionDescription { get; set; }
    public int? ActionTypeId { get; set; }
    public string? ActionTypeName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public int CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public string? CreatedByName { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedByName { get; set; }
}
