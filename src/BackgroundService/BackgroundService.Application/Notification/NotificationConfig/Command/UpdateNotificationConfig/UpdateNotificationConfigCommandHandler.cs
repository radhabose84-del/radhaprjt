
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace  BackgroundService.Application.Notification.NotificationConfig.Command.UpdateNotificationConfig
{
    public class UpdateNotificationConfigCommandHandler  : IRequestHandler<UpdateNotificationConfigCommand, int>
    {
        private readonly INotificationConfigCommandRepository _notificationConfigCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        
        public UpdateNotificationConfigCommandHandler(INotificationConfigCommandRepository notificationConfigCommandRepository, IMediator mediator, IMapper mapper)
        {                        
            _notificationConfigCommandRepository = notificationConfigCommandRepository;
            _mediator = mediator;
            _mapper = mapper;            
        }

        public async Task<int> Handle(UpdateNotificationConfigCommand request, CancellationToken cancellationToken)
        {       
            var notificationConfig = _mapper.Map<Domain.Entities.Notification.NotificationConfig>(request);
            var result = await _notificationConfigCommandRepository.UpdateAsync(request.Id, notificationConfig);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: notificationConfig.Id.ToString(),
                actionName: notificationConfig.ModuleName,
                details: $"Notification Config was updated",
                module: "NotificationConfig");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("Notification Config update failed.");   
        }
    }
}