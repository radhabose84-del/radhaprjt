using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.Domain.Entities
{
    public class MaintenanceCategory : BaseEntity
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        // public ICollection<WorkOrder>? WorkOrderType  {get; set;} 
    }
}