namespace MaintenanceManagement.Domain.Entities.WorkOrderMaster
{
    public class WorkOrderActivity
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public WorkOrder WOActivity { get; set; } = null!; 
        public int ActivityId { get; set; }
        public ActivityMaster ActivityMaster { get; set; } = null!;          
        public string? Description { get; set; }
    }
}