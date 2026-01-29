using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler
{
    public class PreventiveSchedulerDtlUpdateDto
    {
        public int MachineId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly NextDueDate { get; set; }
    }
}