using BackgroundService.Application.Notification.Common.HttpResponse;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate
{
    public class GetAllNotificationTemplateQuery : IRequest<ApiResponseDTO<List<NotificationTemplateDto>>>
    {        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}