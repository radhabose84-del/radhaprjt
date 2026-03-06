using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate
{
    public class CreateNotificationTemplateCommandHandler : IRequestHandler<CreateNotificationTemplateCommand, int>
    {
        private readonly INotificationTemplateCommandRepository _NotificationTemplateRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateNotificationTemplateCommandHandler(INotificationTemplateCommandRepository NotificationTemplateRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateRepository = NotificationTemplateRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {
            var NotificationTemplate = _mapper.Map<Domain.Entities.Notification.NotificationTemplate>(request);
            var result = await _NotificationTemplateRepository.CreateAsync(NotificationTemplate);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: NotificationTemplate.Id.ToString(),
                actionName: NotificationTemplate.NotificationConfigId.ToString(),
                details: $"Notification Template details was created",
                module: "NotificationTemplate");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("Notification Template Creation Failed.");
        }
    }

}
