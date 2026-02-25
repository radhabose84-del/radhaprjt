using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePO
{


    public class GetServicePOByIdQuery : IRequest<PurchaseOrderServiceDetailDto?>
    {


        public int Id { get; set; }
    public GetServicePOByIdQuery(int id) => Id = id;
    }
}