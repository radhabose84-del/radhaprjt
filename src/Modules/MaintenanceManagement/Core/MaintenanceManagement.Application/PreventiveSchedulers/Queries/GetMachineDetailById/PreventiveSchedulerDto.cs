using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class PreventiveSchedulerDto
    {
        public int Id { get; set; }
        public string PreventiveSchedulerName { get; set; }
        public string GroupName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int FrequencyInterval { get; set; }
        public int GraceDays { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        public int IsDownTimeRequired { get; set; }
        public decimal DownTimeEstimateHrs { get; set; }

        public required List<MachineDetailActivityDto> Activity { get; set; }
        public List<MachineDetailItemsDto>? Items { get; set; }
        public required List<MachineDetailBySchedulerIdDto> PreventiveSchedulerDtl { get; set; }
    }
}