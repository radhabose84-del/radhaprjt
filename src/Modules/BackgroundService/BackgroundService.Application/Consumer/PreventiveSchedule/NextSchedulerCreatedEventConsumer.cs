using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Commands.Hangfire;
using Contracts.Events.Maintenance;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule
{
    public class NextSchedulerCreatedEventConsumer : IConsumer<NextSchedulerCreatedEvent>
    {
        private readonly ILogger<NextSchedulerCreatedEventConsumer> _logger;
        private readonly IInboxRepository _inbox;

        public NextSchedulerCreatedEventConsumer(
            ILogger<NextSchedulerCreatedEventConsumer> logger,
            IInboxRepository inbox)
        {
            _logger = logger;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<NextSchedulerCreatedEvent> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(NextSchedulerCreatedEventConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            _logger.LogInformation(
                "Received NextSchedulerCreatedEvent. SchedulerDetailId={DetailId}, DelayInMinutes={Delay}",
                context.Message.PreventiveSchedulerDetailId,
                context.Message.DelayInMinutes);

            await context.Publish(new HangfireWorkOrderScheduleCommand
            {
                CorrelationId = context.Message.CorrelationId,
                SchedulerId = context.Message.PreventiveSchedulerDetailId,
                DelayInMinutes = context.Message.DelayInMinutes
            });

            await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
        }
    }
}
