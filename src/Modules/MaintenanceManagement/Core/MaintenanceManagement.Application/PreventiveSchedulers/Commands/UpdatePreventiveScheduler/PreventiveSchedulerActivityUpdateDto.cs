using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler
{
    public class PreventiveSchedulerActivityUpdateDto
    {
        public int PreventiveSchedulerHeaderId { get; set; }
        public int ActivityId { get; set; }
    }
}