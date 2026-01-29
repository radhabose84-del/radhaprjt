using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.IHangfire;
using Contracts.Commands.Maintenance.PreventiveScheduler;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule
{
    public class RollBackScheduleWorkOrderConsumer : IConsumer<RollbackPreventiveCommand>
    {
        private readonly ILogger<RollBackScheduleWorkOrderConsumer> _logger;
        private readonly IHangfireQuery _hangfireQuery;
        public RollBackScheduleWorkOrderConsumer(ILogger<RollBackScheduleWorkOrderConsumer> logger, IHangfireQuery hangfireQuery)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
        }
        public async Task Consume(ConsumeContext<RollbackPreventiveCommand> context)
        {
            
            foreach (var detail in context.Message.ScheduleDetail)
            {
                var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(detail.Id);
                foreach (var id in hangfireJob)
                {
                    BackgroundJob.Delete(id.ToString());
                }
            }
        }
    }
}