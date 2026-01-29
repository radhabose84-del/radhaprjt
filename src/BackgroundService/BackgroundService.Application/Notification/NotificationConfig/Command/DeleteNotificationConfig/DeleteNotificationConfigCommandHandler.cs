using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Command.DeleteNotificationConfig
{
    public class DeleteNotificationConfigCommandHandler : IRequestHandler<DeleteNotificationConfigCommand, int>
    {
        private readonly INotificationConfigCommandRepository _notificationConfigCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteNotificationConfigCommandHandler(INotificationConfigCommandRepository notificationConfigCommandRepository, IMediator mediator, IMapper mapper)
        {
            _notificationConfigCommandRepository = notificationConfigCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(DeleteNotificationConfigCommand request, CancellationToken cancellationToken)
        {            
            var notificationConfig = _mapper.Map<Domain.Entities.Notification.NotificationConfig>(request);
            var result = await _notificationConfigCommandRepository.DeleteAsync(request.Id,notificationConfig);          

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: notificationConfig.Id.ToString(),
                actionName: notificationConfig.ModuleName,
                details: $"Notification Config  details was deleted",
                module: "NotificationConfig ");
            await _mediator.Publish(domainEvent);
            return result > 0 ? result : throw new ExceptionRules("Notification Config was not found.");
        }


    }
}