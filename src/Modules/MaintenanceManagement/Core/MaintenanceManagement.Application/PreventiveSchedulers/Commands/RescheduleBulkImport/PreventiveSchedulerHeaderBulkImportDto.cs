namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class PreventiveSchedulerHeaderBulkImportDto
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
        public int UnitId { get; set; }
        public int CompanyId { get; set; }
        public List<PrevetiveSchedulerDetailBulkImportDto> PreventDetails { get; set; } = default!;
        public List<PreventiveSchedulerBulkImprotActivityDto> PreventActivities { get; set; } = default!;
    }
}