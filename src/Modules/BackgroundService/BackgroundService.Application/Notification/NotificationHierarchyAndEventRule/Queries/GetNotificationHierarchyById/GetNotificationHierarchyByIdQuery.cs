using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetNotificationHierarchyById
{
    public class GetNotificationHierarchyByIdQuery : IRequest<NotificationHierarchyAndEventRuleDto>
    {
        public int Id { get; set; }       
    }
}
