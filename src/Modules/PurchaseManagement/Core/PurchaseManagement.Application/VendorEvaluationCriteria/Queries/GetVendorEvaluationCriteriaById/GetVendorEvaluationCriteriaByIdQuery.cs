using MediatR;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;

namespace PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaById
{
    public class GetVendorEvaluationCriteriaByIdQuery : IRequest<VendorEvaluationCriteriaDto?>
    {
        public int Id { get; set; }
    }
}
