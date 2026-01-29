using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance.Preventive
{
    public class WorkOrderActivitySagaDto
    {
        public int WorkOrderId { get; set; }
        public int ActivityId { get; set; } 
    }
}