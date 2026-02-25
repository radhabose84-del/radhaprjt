using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO
{
    public class GetVendorServicePOQuery : IRequest<List<GetVendorServicePODto>>
    {
        public int VendorId { get; set; }
    }
}