using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterDto
    {
       
        public string? ActivityName { get; set;}
        public string? Description { get; set; }
        public int  UnitId  {get; set; }
        public int DepartmentId { get; set; }
        public int EstimatedDuration { get; set; }
        public int ActivityType { get; set; }
    

        public List<ActivityMachineGroupDto>? ActivityMachineGroup { get; set; }


    }
}