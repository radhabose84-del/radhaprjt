namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorEvaluationHistoryDto
{
    public int VendorId { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }
    public decimal CurrentScore { get; set; }
    public string? CurrentGradeCode { get; set; }
    public string? CurrentGradeName { get; set; }
    public List<EvaluationHistoryItemDto>? EvaluationHistory { get; set; }
}
