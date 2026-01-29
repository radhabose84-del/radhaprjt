using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.Domain.Entities
{
    public class ActivityCheckListMaster : BaseEntity
    {
        public int   ActivityId { get; set; }
        public string? ActivityCheckList { get; set; }        
        public int  UnitId { get; set; }        
        public ActivityMaster? ActivityMaster { get; set; }  
        public ICollection<WorkOrderCheckList>? WOCheckLists {get; set;} 

        
    }
}