using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Domain.Entities
{
    public class PreventiveSchedulerActivity
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public required PreventiveSchedulerHeader PreventiveScheduler { get; set; }
        public int ActivityId { get; set; }
        public required ActivityMaster Activity { get; set; }
    }
}