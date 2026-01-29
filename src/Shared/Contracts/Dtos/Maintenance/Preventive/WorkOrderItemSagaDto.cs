using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance.Preventive
{
    public class WorkOrderItemSagaDto
    {
        public int WorkOrderId { get; set; }       
        public string OldItemCode { get; set; }
    }
}