using MediatR;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule
{
    public class InsertNotificationHierarchyAndEventRuleCommand : IRequest<bool>
    {
        public NotificationHierarchyAndEventRuleDto Dto { get; set; }

        public InsertNotificationHierarchyAndEventRuleCommand(NotificationHierarchyAndEventRuleDto dto)
        {
            Dto = dto;
        }
    }
}
