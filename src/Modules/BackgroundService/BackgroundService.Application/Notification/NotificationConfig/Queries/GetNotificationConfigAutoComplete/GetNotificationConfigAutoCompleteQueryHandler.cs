
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete
{
    public class GetNotificationConfigAutoCompleteQueryHandler : IRequestHandler<GetNotificationConfigAutoCompleteQuery,List<NotificationConfigAutoCompleteDto>>
    {
        private readonly INotificationConfigQueryRepository _notificationConfigQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetNotificationConfigAutoCompleteQueryHandler(INotificationConfigQueryRepository notificationConfigQueryRepository, IMediator mediator, IMapper mapper)
        {
            _notificationConfigQueryRepository = notificationConfigQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<NotificationConfigAutoCompleteDto>> Handle(GetNotificationConfigAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _notificationConfigQueryRepository.GetNotificationConfigAutoCompleteAsync(request.SearchPattern ?? string.Empty);
            var notificationConfig = _mapper.Map<List<NotificationConfigAutoCompleteDto>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetNotificationConfigAutoCompleteQueryHandler",        
                actionName: notificationConfig.Count.ToString(),
                details: $"Notification Config details was fetched.",
                module:"NotificationConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return notificationConfig;
        }
    }
}