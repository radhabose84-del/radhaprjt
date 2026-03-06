
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace  BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate
{
    public class UpdateNotificationTemplateCommandHandler  : IRequestHandler<UpdateNotificationTemplateCommand, int>
    {
        private readonly INotificationTemplateCommandRepository _NotificationTemplateCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateNotificationTemplateCommandHandler(INotificationTemplateCommandRepository NotificationTemplateCommandRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateCommandRepository = NotificationTemplateCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateNotificationTemplateCommand request, CancellationToken cancellationToken)
        {       
            var NotificationTemplate = _mapper.Map<Domain.Entities.Notification.NotificationTemplate>(request);
            var result = await _NotificationTemplateCommandRepository.UpdateAsync(request.Id, NotificationTemplate);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: NotificationTemplate.Id.ToString(),
                actionName: NotificationTemplate.NotificationConfigId.ToString(),
                details: $"Notification Template was updated",
                module: "NotificationTemplate");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("Notification Template update failed.");   
        }
    }
}