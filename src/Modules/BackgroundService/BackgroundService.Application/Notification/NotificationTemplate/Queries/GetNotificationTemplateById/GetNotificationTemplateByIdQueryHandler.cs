using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateById
{
    public class GetNotificationTemplateByIdQueryHandler : IRequestHandler<GetNotificationTemplateByIdQuery, NotificationTemplateDto>
    {

        private readonly INotificationTemplateQueryRepository _NotificationTemplateQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetNotificationTemplateByIdQueryHandler(INotificationTemplateQueryRepository NotificationTemplateQueryRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationTemplateQueryRepository = NotificationTemplateQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<NotificationTemplateDto> Handle(GetNotificationTemplateByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _NotificationTemplateQueryRepository.GetByIdAsync(request.Id);     
            if (result == null)
            {
                throw new KeyNotFoundException($"NotificationTemplate with Id {request.Id} not found.");
            }      
            var NotificationTemplate = _mapper.Map<NotificationTemplateDto>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetNotificationTemplateByIdQuery",
                actionName: NotificationTemplate.Id.ToString(),
                details: $"NotificationTemplate details {NotificationTemplate.Id} was fetched.",
                module: "NotificationTemplate"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return NotificationTemplate;
        }

    }
}