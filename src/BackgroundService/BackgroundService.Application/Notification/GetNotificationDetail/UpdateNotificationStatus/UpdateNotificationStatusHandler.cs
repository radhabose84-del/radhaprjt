
using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Domain.Events;
using MediatR;

namespace  BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus
{
    public class UpdateNotificationStatusHandler  : IRequestHandler<UpdateNotificationStatus, int>
    {
        private readonly INotificationDetailRepository _NotificationDetailRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateNotificationStatusHandler(INotificationDetailRepository NotificationDetailRepository, IMediator mediator, IMapper mapper)
        {
            _NotificationDetailRepository = NotificationDetailRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateNotificationStatus request, CancellationToken cancellationToken)
        {       
            var NotificationDetail = _mapper.Map<Domain.Entities.Notification.NotificationEventLog>(request);
            var result = await _NotificationDetailRepository.UpdateAsync(request.Id, NotificationDetail);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: NotificationDetail.Id.ToString(),
                actionName: NotificationDetail.MessageText,
                details: $"Notification Log was updated",
                module: "NotificationDetail");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("Notification Log update failed.");   
        }
    }
}