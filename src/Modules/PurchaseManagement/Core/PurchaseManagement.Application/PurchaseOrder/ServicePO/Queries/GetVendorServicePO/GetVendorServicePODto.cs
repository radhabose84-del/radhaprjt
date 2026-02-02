using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetVendorServicePO
{
    public class GetVendorServicePODto
    {
        public int ServicePOId { get; set; }
        public string? ServicePONumber { get; set; }
        public DateTimeOffset ServicePODate { get; set; }
    }
}