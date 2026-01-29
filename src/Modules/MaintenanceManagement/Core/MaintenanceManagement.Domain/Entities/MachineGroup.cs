using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class MachineGroup : BaseEntity

    {
        public string? GroupName { get; set; }
        public int Manufacturer { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public bool PowerSource { get; set; }
        public ICollection<ActivityMachineGroup>? ActivityMachineGroups { get; set; }
        public ICollection<MachineGroupUser>? MachineGroupUser { get; set; }
        public ICollection<MachineMaster>? MachineMasters { get; set; }
        public ICollection<PreventiveSchedulerHeader>? PreventiveSchedulerHeaders { get; set; }   
       
        
    }
}