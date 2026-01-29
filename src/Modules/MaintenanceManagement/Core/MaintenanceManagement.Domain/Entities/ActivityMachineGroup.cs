using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Domain.Entities
{
    public class ActivityMachineGroup
    {    
        public int Id { get; set; }
         public int ActivityMasterId { get; set; }
        public ActivityMaster? ActivityMaster { get; set; } // Navigation Property

        public int MachineGroupId { get; set; }
        public MachineGroup? MachineGroup { get; set; } // Navigation Property

    }
}