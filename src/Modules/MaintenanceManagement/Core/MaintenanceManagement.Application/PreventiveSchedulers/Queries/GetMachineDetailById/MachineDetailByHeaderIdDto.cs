using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailByHeaderIdDto
    {
        public int HeaderId { get; set; }
        public int DetailId { get; set; }
        public int WorkOrderId { get; set; }
        public string PreventiveSchedulerName { get; set; }
        public int MachineGroupId { get; set; }
        public string GroupName { get; set; }
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string MachineCode { get; set; }
    }
}