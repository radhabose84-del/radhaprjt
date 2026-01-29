using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerHdrByIdDto
    {
        public int Id { get; set; }
        public string PreventiveSchedulerName { get; set; }
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
        public int IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }
        public byte IsActive { get; set; }
        public required List<PreventiveSchedulerActivityByIdDto> Activity { get; set; }
        public List<PreventiveSchedulerItemByIdDto>? Items { get; set; }
        public required List<PreventiveSchedulerDtlByIdDto> PreventiveSchedulerDtl { get; set; }
    }
}