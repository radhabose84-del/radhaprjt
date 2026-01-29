using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Maintenance.Preventive
{
    public class ScheduleDetailUpdateDto
    {
        public int Id { get; set; }
        public int DelayInMinutes { get; set; }
    }
}