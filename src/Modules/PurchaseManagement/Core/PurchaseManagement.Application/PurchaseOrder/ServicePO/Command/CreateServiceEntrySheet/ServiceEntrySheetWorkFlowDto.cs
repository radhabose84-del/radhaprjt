using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class ServiceEntrySheetWorkFlowDto
    {
        public int Id { get; set; }               // SES Id
        public int PurchaseOrderId { get; set; }  // link back to PO
        public int VendorId { get; set; }
        public int StatusId { get; set; }         // SES Status / ApprovalStatus
        public int UnitId { get; set; }
    }
}