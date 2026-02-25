using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup
{
    public class MachineGroupDto 
    {        
        public int Id { get; set; }
        public string? GroupName { get; set; }  
        public int Manufacturer  { get; set;}
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; } 
        public byte PowerSource { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

    }
}