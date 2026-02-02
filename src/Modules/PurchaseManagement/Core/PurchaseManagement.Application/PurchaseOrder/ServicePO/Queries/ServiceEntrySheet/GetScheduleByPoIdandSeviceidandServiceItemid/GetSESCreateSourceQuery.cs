using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetScheduleByPoIdandSeviceidandServiceItemid
{
    public class GetSESCreateSourceQuery: IRequest<SesFromScheduleRawDto?>
    {
        public int PurchaseOrderId { get; set; }
        public int ScheduleNo { get; set; }
        public int ServiceItemId { get; set; }
        
    }
}