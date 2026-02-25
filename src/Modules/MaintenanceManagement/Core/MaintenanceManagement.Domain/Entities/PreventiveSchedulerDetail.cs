using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.Domain.Entities
{

    public class PreventiveSchedulerDetail : BaseEntity
    {

        public int PreventiveSchedulerHeaderId { get; set; }
        public required PreventiveSchedulerHeader PreventiveScheduler { get; set; }
        public int MachineId { get; set; }
        public required MachineMaster Machine { get; set; }

        public DateOnly WorkOrderCreationStartDate { get; set; }
        public DateOnly? ActualWorkOrderDate { get; set; }
        public DateOnly MaterialReqStartDays { get; set; }
        public string? RescheduleReason { get; set; }
        public ICollection<WorkOrder>? workOrdersSchedule { get; set; }
        public string? HangfireJobId { get; set; }
        public DateOnly? LastMaintenanceActivityDate { get; set; }
        public int? ScheduleId { get; set; }
        public  MiscMaster? MiscSchedule { get; set; }
        public int? FrequencyTypeId { get; set; }
        public  MiscMaster? MiscFrequencyType { get; set; }
        public int FrequencyInterval { get; set; }
        public int? FrequencyUnitId { get; set; }
        public  MiscMaster? MiscFrequencyUnit { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public byte IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }
        
    }
}