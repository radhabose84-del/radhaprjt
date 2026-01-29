using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Notifications
{
    public class ScheduleWorkOrderBackgroundCommand
    {
        public int PreventiveScheduleId { get; set; }
        public int DelayInMinutes { get; set; }
    }
}