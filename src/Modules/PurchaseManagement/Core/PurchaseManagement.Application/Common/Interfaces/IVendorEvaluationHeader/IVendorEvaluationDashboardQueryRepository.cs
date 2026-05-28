using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader
{
    public interface IVendorEvaluationDashboardQueryRepository
    {
        Task<VendorEvaluationDashboardDto?> GetDashboardAsync(
            int vendorId, int evaluationMonth, int evaluationYear, int lookbackMonths);

        Task<(List<VendorRatingDashboardListItemDto>, int, VendorRatingDashboardSummaryDto)> GetAllDashboardAsync(
            int pageNumber, int pageSize, string? searchTerm, string? grade);

        Task<VendorEvaluationHistoryDto?> GetEvaluationHistoryAsync(int vendorId);
    }
}
