using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Interfaces;
using Contracts.Commands.Hangfire;
using Contracts.Events.Hangfire.PreventiveScheduler;
using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule
{
    public class ScheduleWorkOrderConsumer : IConsumer<HangfireWorkOrderScheduleCommand>
    {
        private readonly ILogger<ScheduleWorkOrderConsumer> _logger;
        private readonly IHangfireQuery _hangfireQuery;
        private readonly IMaintenance _maintenance;
        private readonly IInboxRepository _inbox;

        public ScheduleWorkOrderConsumer(ILogger<ScheduleWorkOrderConsumer> logger, IHangfireQuery hangfireQuery, IMaintenance maintenance, IInboxRepository inbox)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _maintenance = maintenance;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<HangfireWorkOrderScheduleCommand> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(ScheduleWorkOrderConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            try
            {
                _logger.LogInformation("Received request to schedule PreventiveScheduleId: {Id}",
             context.Message.SchedulerId);

                var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(context.Message.SchedulerId);

                foreach (var id in hangfireJob)
                {
                    BackgroundJob.Delete(id.ToString());
                }

                string jobId = await _maintenance.ScheduleWorkOrderJob(context.Message.SchedulerId, context.Message.DelayInMinutes);

                _logger.LogInformation("Scheduled Hangfire Job ID: {JobId}", jobId);

                await context.Publish(new HangfireWorkOrderScheduleEvent
                {
                    CorrelationId = context.Message.CorrelationId
                });

                await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling PreventiveScheduleId: {Id}", context.Message.SchedulerId);
                await context.Publish(new HangfireWorkOrderScheduleFailedEvent
                {
                     CorrelationId = context.Message.CorrelationId,
                    // SchedulerId = context.Message.SchedulerId
                    Reason = ex.Message
                });
            }
           
            
        }
    }
}