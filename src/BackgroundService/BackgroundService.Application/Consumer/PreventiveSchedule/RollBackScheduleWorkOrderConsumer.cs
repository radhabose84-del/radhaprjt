using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.IHangfire;
using BackgroundService.Application.Interfaces.IInbox;
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
        private readonly IInboxRepository _inbox;

        public RollBackScheduleWorkOrderConsumer(ILogger<RollBackScheduleWorkOrderConsumer> logger, IHangfireQuery hangfireQuery, IInboxRepository inbox)
        {
            _logger = logger;
            _hangfireQuery = hangfireQuery;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<RollbackPreventiveCommand> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(RollBackScheduleWorkOrderConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            foreach (var detail in context.Message.ScheduleDetail)
            {
                var hangfireJob = await _hangfireQuery.GetHangfireJobByTransactionId(detail.Id);
                foreach (var id in hangfireJob)
                {
                    BackgroundJob.Delete(id.ToString());
                }
            }

            await _inbox.MarkAsProcessedAsync(consumerName, messageId, null, context.CancellationToken);
        }
    }
}