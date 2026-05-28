namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class GradeReferenceDto
{
    public int Id { get; set; }
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public decimal MinScore { get; set; }
    public decimal MaxScore { get; set; }
    public string? ActionDescription { get; set; }
}
