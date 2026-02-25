using MediatR;

namespace UserManagement.Application.Notification.Queries
{
    public class NotificationRequest : IRequest<NotificationResponse>
    {
        public string? Username { get; set; }
    }
}