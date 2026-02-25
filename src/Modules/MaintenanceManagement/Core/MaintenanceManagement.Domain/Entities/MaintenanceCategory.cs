using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class MaintenanceCategory : BaseEntity
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        // public ICollection<WorkOrder>? WorkOrderType  {get; set;} 
    }
}