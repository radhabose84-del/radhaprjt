using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById
{
    public class GetNotificationDetailByUserIdHandler : IRequestHandler<GetNotificationDetailByUserId, List<GetNotificationDetailDto>>
    {
        private readonly INotificationDetailRepository _notificationDetailRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetNotificationDetailByUserIdHandler(
            INotificationDetailRepository notificationDetailRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _notificationDetailRepository = notificationDetailRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<GetNotificationDetailDto>> Handle(GetNotificationDetailByUserId request, CancellationToken cancellationToken)
        {
            var userId = (request.UserId ?? string.Empty).Trim();

            var entities = await _notificationDetailRepository.GetAllByUserIdAsync(userId);

            var result = entities?.ToList() ?? new List<GetNotificationDetailDto>();

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllByUserId",
                actionCode: "GetNotificationDetailByUserIdQuery",
                actionName: userId,
                details: $"Fetched {result.Count} notifications for UserId {userId}.",
                module: "NotificationDetail"), cancellationToken);

            return result; 
        }
    }
    

}