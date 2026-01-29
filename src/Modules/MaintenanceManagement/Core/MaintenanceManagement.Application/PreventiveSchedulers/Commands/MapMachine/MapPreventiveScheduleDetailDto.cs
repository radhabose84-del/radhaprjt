using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine
{
    public class MapPreventiveScheduleDetailDto
    {
        public int PreventiveSchedulerHeaderId { get; set; }
        public int MachineId { get; set; }
        public DateOnly WorkOrderCreationStartDate { get; set; }
        public DateOnly? ActualWorkOrderDate { get; set; }
        public DateOnly MaterialReqStartDays { get; set; }
        public string? HangfireJobId { get; set; }
        public int? ScheduleId { get; set; }
        public int? FrequencyTypeId { get; set; }
        public int FrequencyInterval { get; set; }
        public int? FrequencyUnitId { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public byte IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }

       
    }
}