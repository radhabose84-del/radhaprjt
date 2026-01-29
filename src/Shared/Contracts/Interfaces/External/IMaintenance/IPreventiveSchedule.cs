using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Interfaces.External.IMaintenance
{
    public interface IPreventiveSchedule
    {
        Task<bool> ScheduleWorkOrder();
    }
}