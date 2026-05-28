namespace PurchaseManagement.Application.VendorEvaluationHeader.Dto;

public class VendorRatingDashboardResponseDto
{
    public VendorRatingDashboardSummaryDto? Summary { get; set; }
    public List<VendorRatingDashboardListItemDto>? Items { get; set; }
}
