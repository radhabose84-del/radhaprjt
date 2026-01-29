using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById
{
    public class GetMachineGroupNameByIdDto
    {
        public int ActivityId { get; set;}
        public int MachineGroupId { get; set;}
        public string? MachineGroupName { get; set;}
    }
}