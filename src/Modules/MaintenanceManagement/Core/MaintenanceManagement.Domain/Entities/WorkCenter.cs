using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class WorkCenter :BaseEntity
    {
        public string? WorkCenterCode { get; set; }
        public string? WorkCenterName { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public ICollection<MachineMaster>? MachineMasters { get; set; } 
       
    }
}