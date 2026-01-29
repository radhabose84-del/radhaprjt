using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetSchedulerByDate
{
    public class SchedulerByDateDto
    {
        public int TotalScheduleCount { get; set; }
        public string ScheduleDate { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}