using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class PreventiveSchedulerHeader : BaseEntity
    {
        public string? PreventiveSchedulerName { get; set; }
        public int MachineGroupId { get; set; }
        public required MachineGroup MachineGroup { get; set; }
        public int DepartmentId { get; set; }
        public int MaintenanceCategoryId { get; set; }
        public required MiscMaster MiscMaintenanceCategory { get; set; }
        public int ScheduleId { get; set; }
        public required MiscMaster MiscSchedule { get; set; }
        public int FrequencyTypeId { get; set; }
        public required MiscMaster MiscFrequencyType { get; set; }
        public int 	FrequencyInterval { get; set; }
        public int FrequencyUnitId { get; set; }
        public required MiscMaster MiscFrequencyUnit { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public byte IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }
        public int UnitId { get; set; }
        public int CompanyId { get; set; }
        public ICollection<PreventiveSchedulerDetail>? PreventiveSchedulerDetails { get; set; }
        public ICollection<PreventiveSchedulerActivity>? PreventiveSchedulerActivities { get; set; }
        public ICollection<PreventiveSchedulerItems>? PreventiveSchedulerItems { get; set; }
    }
}