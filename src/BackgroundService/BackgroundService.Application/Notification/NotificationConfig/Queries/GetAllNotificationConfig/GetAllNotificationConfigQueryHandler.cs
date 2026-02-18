using AutoMapper;
using Contracts.Common;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Domain.Events;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig
{
    public class GetAllNotificationConfigQueryHandler : IRequestHandler<GetAllNotificationConfigQuery, ApiResponseDTO<List<NotificationConfigDto>>>
    {
        private readonly INotificationConfigQueryRepository _notificationConfigQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetAllNotificationConfigQueryHandler(INotificationConfigQueryRepository notificationConfigQueryRepository, IMediator mediator, IMapper mapper)
        {
            _notificationConfigQueryRepository = notificationConfigQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<List<NotificationConfigDto>>> Handle(GetAllNotificationConfigQuery request, CancellationToken cancellationToken)
        {
            var (notificationConfig, totalCount) = await _notificationConfigQueryRepository.GetAllNotificationConfigAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var notificationConfigDto = _mapper.Map<List<NotificationConfigDto>>(notificationConfig);

            // 📘 Log domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetNotificationConfig",
                actionCode: "Get",
                actionName: notificationConfig.Count().ToString(),
                details: "Notification details were fetched.",
                module: "NotificationConfig"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // ✅ Return
            return new ApiResponseDTO<List<NotificationConfigDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = notificationConfigDto,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }



    }
}