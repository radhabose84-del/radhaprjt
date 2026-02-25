using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServiceScheduleByPoAndServiceId
{
    public class GetByPoAndServiceIdQuery : IRequest<List<ServiceScheduleDto>>
    {

        public int  PoId { get; set; }
        public int ServiceId { get; set; }
        
    }
}