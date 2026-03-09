using BackgroundService.Application.Interfaces.IInbox;
using Contracts.Commands.Maintenance.PreventiveScheduler;
using Contracts.Events.Maintenance.PreventiveScheduler;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumer.PreventiveSchedule
{
    public class MachineWiseScheduleCreationConsumer : IConsumer<MachineWiseScheduleCreationEvent>
    {
        private readonly ILogger<MachineWiseScheduleCreationConsumer> _logger;
        private readonly IInboxRepository _inbox;

        public MachineWiseScheduleCreationConsumer(
            ILogger<MachineWiseScheduleCreationConsumer> logger,
            IInboxRepository inbox)
        {
            _logger = logger;
            _inbox = inbox;
        }

        public async Task Consume(ConsumeContext<MachineWiseScheduleCreationEvent> context)
        {
            var messageId = context.MessageId ?? Guid.NewGuid();
            const string consumerName = nameof(MachineWiseScheduleCreationConsumer);

            if (await _inbox.IsAlreadyProcessedAsync(consumerName, messageId, context.CancellationToken))
            {
                _logger.LogInformation(
                    "Inbox dedup: duplicate skipped. Consumer={Consumer}, MessageId={MessageId}",
                    consumerName, messageId);
                return;
            }

            _logger.LogInformation(
                "Received MachineWiseScheduleCreationEvent. HeaderId={HeaderId}, Details={Count}",
                context.Message.PreventiveSchedulerHeaderId,
                context.Message.ScheduleDetail?.Count ?? 0);

            await context.Publish(new SheduleWorkOrderCommand
            {
                CorrelationId = context.Message.CorrelationId,
                PreventiveSchedulerHeaderId = context.Message.PreventiveSchedulerHeaderId,
                ScheduleDetail = context.Message.ScheduleDetail
            });

            await _inbox.MarkAsProcessedAsync(consumerName, messageId, context.Message.CorrelationId, context.CancellationToken);
        }
    }
}
