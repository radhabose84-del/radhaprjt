using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines
{
    public class GetServicePOLinesQuery : IRequest<IReadOnlyList<GetServicePOLinesDto>>
    {
       public int POId { get; set; }
    }
}