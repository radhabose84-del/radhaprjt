using MediatR;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationDashboard
{
    public class GetVendorEvaluationDashboardQuery : IRequest<VendorEvaluationDashboardDto?>
    {
        public int VendorId { get; set; }
        public int EvaluationMonth { get; set; }
        public int EvaluationYear { get; set; }
        public int LookbackMonths { get; set; } = 3;
    }
}
