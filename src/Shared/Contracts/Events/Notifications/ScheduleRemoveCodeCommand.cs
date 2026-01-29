using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Notifications
{
    public class ScheduleRemoveCodeCommand
    {
        public string UserName { get; set; } = default!;
        public int DelayInMinutes { get; set; }
    }
}