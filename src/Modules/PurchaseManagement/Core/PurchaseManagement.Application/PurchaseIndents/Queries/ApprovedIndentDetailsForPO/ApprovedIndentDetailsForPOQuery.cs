using MediatR;

namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
{
    public class ApprovedIndentDetailsForPOQuery : IRequest<List<IndentForPODto>>
    {
        public int? VendorId { get; set; }
        public int? DepartmentId { get; set; }
    }

}