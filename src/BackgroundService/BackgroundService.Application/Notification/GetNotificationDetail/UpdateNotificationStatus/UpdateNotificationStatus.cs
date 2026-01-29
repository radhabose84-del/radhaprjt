using MediatR;

namespace BackgroundService.Application.Notification.GetNotificationDetail.UpdateNotificationStatus
{
    public class UpdateNotificationStatus : IRequest<int>
    {
        public int Id { get; set; }
        public int ReadStatusId { get; set; }
    }
}