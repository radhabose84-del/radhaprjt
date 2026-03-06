using MediatR;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.DeleteNotificationEventRule
{
    public class DeleteNotificationLevelHierarchyCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
