using MediatR;

namespace BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById
{
    public class GetNotificationDetailByUserId : IRequest<List<GetNotificationDetailDto>>
    {
        public string? UserId { get; set; }
    }
}