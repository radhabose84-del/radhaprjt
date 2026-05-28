using MediatR;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHistory
{
    public class GetVendorEvaluationHistoryQuery : IRequest<VendorEvaluationHistoryDto?>
    {
        public int VendorId { get; set; }
    }
}
