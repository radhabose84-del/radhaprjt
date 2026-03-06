using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete
{
    public class GetNotificationTemplateAutoCompleteQuery : IRequest<List<NotificationTemplateAutoCompleteDto>>    
    {
        public string? SearchPattern { get; set; }       
    }
}