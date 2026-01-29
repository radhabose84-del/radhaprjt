using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using Contracts.Commands.Maintenance.PreventiveScheduler;
using Contracts.Events.Maintenance.PreventiveScheduler;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule
{
    public class NewScheduleWorkOrderTaskConsumer : IConsumer<SheduleWorkOrderCommand>
    {
        private readonly ILogger<NewScheduleWorkOrderTaskConsumer> _logger;
        private readonly IHangfireQuery _hangfireQuery;
        private readonly IMaintenance _maintenance;
        public NewScheduleWorkOrderTaskConsumer(ILogger<NewScheduleWorkOrderTaskConsumer> logger, IHangfireQuery hangfireQuery, IMaintenance maintenance)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _maintenance = maintenance;
        }
        public async Task Consume(ConsumeContext<SheduleWorkOrderCommand> context)
        {
            try
            {



                
                foreach (var detail in context.Message.ScheduleDetail)
                {


                    var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(detail.Id);
                    foreach (var id in hangfireJob)
                    {
                        BackgroundJob.Delete(id.ToString());
                    }
                      await _maintenance.ScheduleWorkOrderJob(detail.Id, detail.DelayInMinutes);

                }
                var headerId = context.Message.PreventiveSchedulerHeaderId;
                if (context.Message.ScheduleDetail.Count > 0)
                {

                    await context.Publish(new ScheduleWorkOrderCreationEvent
                    {
                        CorrelationId = context.Message.CorrelationId
                    });
                }

                else
                {


                    await context.Publish(new ScheduleWorkOrderFailedEvent
                    {
                        CorrelationId = context.Message.CorrelationId
                    });
                }
            }
            catch (Exception ex)
            {

                await context.RespondAsync(new ScheduleWorkOrderFailedEvent
                {
                    CorrelationId = context.Message.CorrelationId,
                    Reason = $"Exception: {ex.Message}"
                });
            }
        }
    }
}