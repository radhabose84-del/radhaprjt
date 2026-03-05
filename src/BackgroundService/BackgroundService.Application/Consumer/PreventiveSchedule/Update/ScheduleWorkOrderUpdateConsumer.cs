using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Interfaces.IInbox;
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
        private readonly IInboxRepository _inbox;

        public ScheduleWorkOrderUpdateConsumer(ILogger<NewScheduleWorkOrderTaskConsumer> logger, IHangfireQuery hangfireQuery, IMaintenance maintenance, IInboxRepository inbox)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _maintenance = maintenance;
            _inbox = inbox;
        }
        public async Task Consume(ConsumeContext<UpdateScheduleWorkOrderCommand> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(ScheduleWorkOrderUpdateConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

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

                await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
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