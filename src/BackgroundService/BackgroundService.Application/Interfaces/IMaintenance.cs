using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;

namespace BackgroundService.Application.Interfaces
{

    public interface IMaintenance
    {
        public Task SchedulerWorkOrderExecute(int PreventiveScheduleId);
        Task<int> GetTotalPendingJobsAsync();
        Task<string> ScheduleWorkOrderJob(int PreventiveScheduleId, int DelayInMinutes);
    }
}