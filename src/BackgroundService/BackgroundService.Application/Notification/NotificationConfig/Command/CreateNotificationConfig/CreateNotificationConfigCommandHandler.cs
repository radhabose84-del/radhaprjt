using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.CreateNotificationConfig
{
    public class CreateNotificationConfigCommandHandler : IRequestHandler<CreateNotificationConfigCommand, int>
    {
        private readonly INotificationConfigCommandRepository _notificationConfigRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateNotificationConfigCommandHandler(INotificationConfigCommandRepository notificationConfigRepository, IMediator mediator, IMapper mapper)
        {
            _notificationConfigRepository = notificationConfigRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateNotificationConfigCommand request, CancellationToken cancellationToken)
        {
            var notificationConfig = _mapper.Map<Domain.Entities.Notification.NotificationConfig>(request);
            var result = await _notificationConfigRepository.CreateAsync(notificationConfig);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: notificationConfig.Id.ToString(),
                actionName: notificationConfig.ModuleName,
                details: $"Notification Config details was created",
                module: "NotificationConfig");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("Notification Config Creation Failed.");
        }
    }

}
