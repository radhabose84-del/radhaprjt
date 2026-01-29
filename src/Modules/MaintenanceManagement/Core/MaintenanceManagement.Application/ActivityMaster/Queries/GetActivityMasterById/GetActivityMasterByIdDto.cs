using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById
{
    public class GetActivityMasterByIdDto
    {
        //  public int Id { get; set;}
        // public string? ActivityName { get; set;}
        // public string? Description { get; set; }
        // public int DepartmentId { get; set; }
        // public string? Department { get; set; } 
        // public int MachineGroupId { get; set; }
        // public string? MachineGroupName { get; set; } 
        // public int EstimatedDuration { get; set; }
        // public int ActivityType { get; set; }
        // public string? ActivityTypeDescription { get; set; }
        //  public Status  IsActive { get; set; }
        // public IsDelete IsDeleted { get; set; }

        public int Id { get; set;}
        public string? ActivityName { get; set;}
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? Department { get; set; }        
        public int EstimatedDuration { get; set; }
        public int ActivityType { get; set; }
        public string? ActivityTypeDescription { get; set; }
         public Status  IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }


         public List<GetAllMachineGroupDto>? GetAllMachineGroupDto { get; set; } 
    }
}