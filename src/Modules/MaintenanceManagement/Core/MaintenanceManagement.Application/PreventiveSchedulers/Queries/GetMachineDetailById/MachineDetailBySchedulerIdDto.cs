using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById
{
    public class MachineDetailBySchedulerIdDto
    {
        public int Id { get; set; }
        public string MachineCode { get; set; }
        public string MachineName { get; set; }
        public DateOnly WorkOrderCreationStartDate { get; set; }
        public DateOnly LastMaintenanceActivityDate { get; set; }
        public DateOnly ActualWorkOrderDate { get; set; }
        public int FrequencyInterval { get; set; }
        public byte IsActive { get; set; }
    }
}