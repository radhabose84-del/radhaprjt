using MediatR;
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetAllNotificationGroupMember;

namespace BackgroundService.Application.Notification.NotificationGroupMember.Queries.GetNotificationGroupById
{
    public class GetNotificationGroupByIdQuery : IRequest<ApiResponseDTO<GetNotificationGroupMemberDto>>
    {
        public int Id { get; set; }
    }
}
