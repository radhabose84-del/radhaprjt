using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById
{
    public class GetNotificationDetailByUserId : IRequest<ApiResponseDTO<List<GetNotificationDetailDto>>>
    {
        public string? UserId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public string? ReadStatus { get; set; }
    }
}