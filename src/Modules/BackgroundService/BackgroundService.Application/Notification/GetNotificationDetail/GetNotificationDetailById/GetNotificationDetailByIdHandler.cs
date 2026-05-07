using AutoMapper;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Domain.Events;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById
{
    public class GetNotificationDetailByUserIdHandler : IRequestHandler<GetNotificationDetailByUserId, ApiResponseDTO<List<GetNotificationDetailDto>>>
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

        public async Task<ApiResponseDTO<List<GetNotificationDetailDto>>> Handle(GetNotificationDetailByUserId request, CancellationToken cancellationToken)
        {
            var userId = (request.UserId ?? string.Empty).Trim();

            var (data, totalCount) = await _notificationDetailRepository.GetAllByUserIdAsync(
                userId, request.PageNumber, request.PageSize,
                request.FromDate, request.ToDate, request.ReadStatus);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAllByUserId",
                actionCode: "GetNotificationDetailByUserIdQuery",
                actionName: userId,
                details: $"Fetched {data.Count} notifications for UserId {userId}.",
                module: "NotificationDetail"), cancellationToken);

            return new ApiResponseDTO<List<GetNotificationDetailDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}