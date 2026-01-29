using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerActivityByIdDto
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHdrId { get; set; }
        public int ActivityId { get; set; }
    }
}