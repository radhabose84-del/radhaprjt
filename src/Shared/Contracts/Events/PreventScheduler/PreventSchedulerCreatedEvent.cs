using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.PreventScheduler
{
    public class PreventSchedulerCreatedEvent
    {
        public Guid CorrelationId { get; set; }
        public int Id { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public int ReminderWorkOrderDays { get; set; }
        public int ReminderMaterialReqDays { get; set; }
        
    }
}