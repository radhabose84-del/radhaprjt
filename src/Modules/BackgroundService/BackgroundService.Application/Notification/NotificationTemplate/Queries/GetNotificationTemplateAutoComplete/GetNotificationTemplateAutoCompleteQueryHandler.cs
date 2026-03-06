
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete
{
    public class GetNotificationTemplateAutoCompleteQueryHandler : IRequestHandler<GetNotificationTemplateAutoCompleteQuery,List<NotificationTemplateAutoCompleteDto>>
    {
        private readonly INotificationTemplateQueryRepository _NotificationTemplateQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetNotificationTemplateAutoCompleteQueryHandler(INotificationTemplateQueryRepository NotificationTemplateQueryRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateQueryRepository = NotificationTemplateQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<NotificationTemplateAutoCompleteDto>> Handle(GetNotificationTemplateAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _NotificationTemplateQueryRepository.GetNotificationTemplateAutoCompleteAsync(request.SearchPattern ?? string.Empty);
            
            var NotificationTemplate = _mapper.Map<List<NotificationTemplateAutoCompleteDto>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetNotificationTemplateAutoCompleteQueryHandler",        
                actionName: NotificationTemplate.Count.ToString(),
                details: $"Notification Template details was fetched.",
                module:"NotificationTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return NotificationTemplate;
        }
    }
}