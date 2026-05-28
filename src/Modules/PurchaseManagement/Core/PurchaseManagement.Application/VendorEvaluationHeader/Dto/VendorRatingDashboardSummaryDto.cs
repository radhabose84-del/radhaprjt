namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorRatingDashboardSummaryDto
{
    public int TotalVendors { get; set; }
    public decimal AverageScore { get; set; }
    public List<GradeCountDto>? GradeDistribution { get; set; }
}

public class GradeCountDto
{
    public string? GradeCode { get; set; }
    public string? GradeName { get; set; }
    public int Count { get; set; }
}
