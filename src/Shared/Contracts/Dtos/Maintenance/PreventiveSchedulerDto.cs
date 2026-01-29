using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance
{
    public class PreventiveSchedulerDto
    {
        public int PreventiveScheduleId { get; set; }
        public int DelayInMinutes { get; set; }
    }
}