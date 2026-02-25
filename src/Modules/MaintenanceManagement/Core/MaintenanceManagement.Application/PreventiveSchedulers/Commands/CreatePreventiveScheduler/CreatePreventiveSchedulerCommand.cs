using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler
{
    public class CreatePreventiveSchedulerCommand : IRequest<int>
    {
        public string PreventiveSchedulerName { get; set; } = default!;
        public int MachineGroupId { get; set; }
        public int DepartmentId { get; set; }
        public int MaintenanceCategoryId { get; set; }
        public int ScheduleId { get; set; }
        public int FrequencyTypeId { get; set; }
        public int FrequencyInterval { get; set; }
        public int FrequencyUnitId { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public byte IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }
        public List<PreventiveSchedulerActivityDto>? Activity { get; set; }
        public List<PreventiveSchedulerItemsDto>? Items { get; set; }
        // public List<PreventiveSchedulerDtlDto>? Machines { get; set; }
    }
}