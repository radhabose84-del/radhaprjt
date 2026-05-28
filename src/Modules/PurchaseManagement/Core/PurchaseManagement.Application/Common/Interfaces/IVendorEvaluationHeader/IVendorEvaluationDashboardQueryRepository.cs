using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader
{
    public interface IVendorEvaluationDashboardQueryRepository
    {
        Task<VendorEvaluationDashboardDto?> GetDashboardAsync(
            int vendorId, int evaluationMonth, int evaluationYear, int lookbackMonths);
    }
}
