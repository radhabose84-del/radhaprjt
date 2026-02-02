using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetServicePOLines
{
    public class GetServicePOLinesDto
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public int ServicePoHeaderId { get; set; }
        public int LineNo { get; set; }
        public int? ServiceId { get; set; }
        public string? ServiceDescription { get; set; }
    }
}