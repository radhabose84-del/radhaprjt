
using BackgroundService.Application.Notification.Common.HttpResponse;
using BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Commands.UpdateNotificationEventRule;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationHierarchyAndEventRule.Queries.GetAllNotificationHierarchy
{
   public class GetAllNotificationHierarchyQuery : IRequest<ApiResponseDTO<List<NotificationHierarchyAndEventRuleDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }  // Optional filter
    }
}
