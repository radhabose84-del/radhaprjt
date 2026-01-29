using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Reports.ScheduleReport
{
    public class ScheduleReportDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string MachineName { get; set; }
        public string GroupName { get; set; }
        public string MaintenanceCategory { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string DueDate { get; set; }
        public string LastCompletionDate { get; set; }
        public string PreventiveSchedulerName { get; set; }
        public string MachineCode { get; set; }
        public string WorkOrderStatus { get; set; }
        public string WorkOrderDocNo { get; set; }
        public int ProductionDepartmentId { get; set; }
        public string ProductionDepartmentName { get; set; }
        public string PendingDays { get; set; }
        public int UnitId { get; set; }
    }
}