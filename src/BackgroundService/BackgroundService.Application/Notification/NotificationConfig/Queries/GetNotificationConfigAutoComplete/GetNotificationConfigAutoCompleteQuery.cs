using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete
{
    public class GetNotificationConfigAutoCompleteQuery : IRequest<List<NotificationConfigAutoCompleteDto>>    
    {
        public string? SearchPattern { get; set; }       
    }
}