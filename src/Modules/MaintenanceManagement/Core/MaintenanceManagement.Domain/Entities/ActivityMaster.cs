using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.Domain.Entities
{
    public class ActivityMaster : BaseEntity
    {      
     
      public string? ActivityName { get; set; }
      public string? Description { get; set; }
      public int UnitId { get; set; }
      public int DepartmentId { get; set; }
    
      public int EstimatedDuration { get; set; }
      public int ActivityType { get; set; } 
      public MiscMaster? ActivityTypes { get; set; } 
      public ICollection<ActivityMachineGroup>? ActivityMachineGroups { get; set; } // ✅ Many-to-Many Relation 
      public ICollection<WorkOrderActivity>? workOrderActivities { get; set; } 

      public ICollection<ActivityCheckListMaster>? ActivityCheckLists { get; set; } // ✅ One-to-Many Relation
      public ICollection<PreventiveSchedulerActivity>? PreventiveSchedulerActivities { get; set; }


    }
}