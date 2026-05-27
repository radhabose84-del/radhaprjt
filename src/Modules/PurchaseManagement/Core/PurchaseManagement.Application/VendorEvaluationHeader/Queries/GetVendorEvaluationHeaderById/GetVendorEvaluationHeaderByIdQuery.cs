using MediatR;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHeaderById
{
    public class GetVendorEvaluationHeaderByIdQuery : IRequest<VendorEvaluationHeaderDto?>
    {
        public int Id { get; set; }
    }
}
