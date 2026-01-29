using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using Contracts.Commands.Maintenance.PreventiveScheduler.Update;
using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule.Update
{
    public class ScheduleWorkOrderUpdateConsumer : IConsumer<UpdateScheduleWorkOrderCommand>
    {
        private readonly ILogger<NewScheduleWorkOrderTaskConsumer> _logger;
        private readonly IHangfireQuery _hangfireQuery;
        private readonly IMaintenance _maintenance;
        public ScheduleWorkOrderUpdateConsumer(ILogger<NewScheduleWorkOrderTaskConsumer> logger, IHangfireQuery hangfireQuery, IMaintenance maintenance)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _maintenance = maintenance;
        }
        public async Task Consume(ConsumeContext<UpdateScheduleWorkOrderCommand> context)
        {
            try
            {
                
                foreach (var detail in context.Message.ScheduleDetailUpdate)
                {

                    var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(detail.Id);
                    foreach (var id in hangfireJob)
                    {
                        BackgroundJob.Delete(id.ToString());
                    }
                    await _maintenance.ScheduleWorkOrderJob(detail.Id, detail.DelayInMinutes);
                }
                
                    


            }
            catch (Exception ex)
            {
                await context.Publish(new UpdateScheduleWorkOrderFailedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = ex.Message

                });
            }
        }
    }
}