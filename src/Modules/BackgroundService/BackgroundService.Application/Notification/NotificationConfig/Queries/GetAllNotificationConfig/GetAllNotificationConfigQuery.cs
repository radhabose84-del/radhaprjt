using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig
{
    public class GetAllNotificationConfigQuery : IRequest<ApiResponseDTO<List<NotificationConfigDto>>>
    {        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}