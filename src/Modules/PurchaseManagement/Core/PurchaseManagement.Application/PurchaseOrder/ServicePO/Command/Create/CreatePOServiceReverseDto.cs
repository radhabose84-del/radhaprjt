using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.Create
{
    public class CreatePOServiceReverseDto
    {
        public POServiceWorkFlowDto Header { get; set; } = default!;
        public List<POServiceWorkFlowDto> Lines { get; set; } = new();
    }
}