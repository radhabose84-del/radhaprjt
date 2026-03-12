using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Commands.Maintenance.PreventiveScheduler.Update;
using Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule.Update
{
    public class HeaderUpdateEventConsumer : IConsumer<HeaderUpdateEvent>
    {
        private readonly ILogger<HeaderUpdateEventConsumer> _logger;
        private readonly IInboxRepository _inbox;

        public HeaderUpdateEventConsumer(
            ILogger<HeaderUpdateEventConsumer> logger,
            IInboxRepository inbox)
        {
            _logger = logger;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<HeaderUpdateEvent> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(HeaderUpdateEventConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            _logger.LogInformation(
                "Received HeaderUpdateEvent. DetailsToReschedule={Count}",
                context.Message.ScheduleDetailUpdate?.Count ?? 0);

            await context.Publish(new UpdateScheduleWorkOrderCommand
            {
                CorrelationId = context.Message.CorrelationId,
                ScheduleDetailUpdate = context.Message.ScheduleDetailUpdate
            });

            await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
        }
    }
}
