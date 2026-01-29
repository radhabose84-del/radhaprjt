using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Command.DeleteNotificationTemplate
{
    public class DeleteNotificationTemplateCommandHandler : IRequestHandler<DeleteNotificationTemplateCommand, int>
    {
        private readonly INotificationTemplateCommandRepository _NotificationTemplateCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteNotificationTemplateCommandHandler(INotificationTemplateCommandRepository NotificationTemplateCommandRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateCommandRepository = NotificationTemplateCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(DeleteNotificationTemplateCommand request, CancellationToken cancellationToken)
        {            
            var NotificationTemplate = _mapper.Map<Domain.Entities.Notification.NotificationTemplate>(request);
            var result = await _NotificationTemplateCommandRepository.DeleteAsync(request.Id,NotificationTemplate);          

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: NotificationTemplate.Id.ToString(),
                actionName: NotificationTemplate.NotificationConfigId.ToString(),
                details: $"Notification Template  details was deleted",
                module: "NotificationTemplate ");
            await _mediator.Publish(domainEvent);
            return result > 0 ? result : throw new ExceptionRules("Notification Template was not found.");
        }


    }
}