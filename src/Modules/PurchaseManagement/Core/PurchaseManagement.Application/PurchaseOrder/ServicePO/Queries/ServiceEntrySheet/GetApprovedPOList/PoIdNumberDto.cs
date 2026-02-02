using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetApprovedPOList
{
    public class PoIdNumberDto
    {
        public int POId { get; set; }
        public string ServicePONumber { get; set; } = "";
    }
}