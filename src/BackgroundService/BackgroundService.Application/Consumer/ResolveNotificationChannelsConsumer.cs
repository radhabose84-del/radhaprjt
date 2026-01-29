using System;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Events.Notifications;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BackgroundService.Application.Consumers
{
    public class ResolveNotificationChannelsConsumer : IConsumer<SendNotificationInternalCommand>
    {
        private readonly INotificationResolverHandler _resolverHandler;
        private readonly ILogger<ResolveNotificationChannelsConsumer> _logger;

        public ResolveNotificationChannelsConsumer(
            INotificationResolverHandler resolverHandler,
            ILogger<ResolveNotificationChannelsConsumer> logger)
        {
            _resolverHandler = resolverHandler;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<SendNotificationInternalCommand> context)
        {
            _logger.LogInformation("Resolver got req | ReqId={RequestId} RespAddr={Resp}",
                context.RequestId, context.ResponseAddress);

            try
            {
                var m = context.Message;

                var channels = await _resolverHandler.ResolveNotificationChannelsAsync(
                    m.UnitId, m.ModuleName, m.EventTypeId,
                    m.Email  ?? string.Empty,
                    m.ccMail ?? string.Empty,
                    m.Mobile ?? string.Empty);

                _logger.LogInformation("Resolved channels => [{Channels}]",
                    string.Join(", ", channels ?? Enumerable.Empty<string>()));

                await context.RespondAsync(new ResolveNotificationChannelsResponse
                {
                    CorrelationId = m.CorrelationId,
                    Channels = channels?.ToList() ?? []
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resolver failed while processing SendNotificationInternalCommand");
                throw; 
            }
        }
    }
}
