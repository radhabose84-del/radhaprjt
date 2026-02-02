using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet
{
    public class CreateServiceEntrySheetWorkflowDto
    {
          public ServiceEntrySheetWorkFlowDto Header { get; set; } = default!;
        public List<ServiceEntrySheetWorkFlowDto> Lines { get; set; } = new();
    }
}