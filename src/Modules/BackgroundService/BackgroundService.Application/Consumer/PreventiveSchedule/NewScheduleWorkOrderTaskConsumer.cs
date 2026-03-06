using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Interfaces.IInbox;
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
        private readonly IInboxRepository _inbox;

        public NewScheduleWorkOrderTaskConsumer(ILogger<NewScheduleWorkOrderTaskConsumer> logger, IHangfireQuery hangfireQuery, IMaintenance maintenance, IInboxRepository inbox)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _maintenance = maintenance;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<SheduleWorkOrderCommand> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(NewScheduleWorkOrderTaskConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

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
                    await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
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