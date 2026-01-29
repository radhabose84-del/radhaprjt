using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigById
{
    public class GetNotificationConfigByIdQueryHandler : IRequestHandler<GetNotificationConfigByIdQuery, NotificationConfigDto>
    {

        private readonly INotificationConfigQueryRepository _notificationConfigQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetNotificationConfigByIdQueryHandler(INotificationConfigQueryRepository notificationConfigQueryRepository, IMediator mediator, IMapper mapper)
        {
            _notificationConfigQueryRepository = notificationConfigQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<NotificationConfigDto> Handle(GetNotificationConfigByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _notificationConfigQueryRepository.GetByIdAsync(request.Id);   
            if (result == null)
            {
                throw new KeyNotFoundException($"NotificationConfig with Id {request.Id} not found.");
            }         
            var notificationConfig = _mapper.Map<NotificationConfigDto>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetNotificationConfigByIdQuery",
                actionName: notificationConfig.Id.ToString(),
                details: $"NotificationConfig details {notificationConfig.Id} was fetched.",
                module: "NotificationConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return notificationConfig;
        }

    }
}