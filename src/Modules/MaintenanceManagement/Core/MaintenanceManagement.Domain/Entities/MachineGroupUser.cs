using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class MachineGroupUser : BaseEntity
    {
        public int MachineGroupId  { get; set; }
        public MachineGroup? MachineGroup { get; set; }
        public int DepartmentId  { get; set; }
        public int UserId  { get; set; }
    }
}