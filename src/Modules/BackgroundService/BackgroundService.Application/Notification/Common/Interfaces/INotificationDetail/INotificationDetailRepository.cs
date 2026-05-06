
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail
{
    public interface INotificationDetailRepository
    {     
        Task<(List<GetNotificationDetailDto> Data, int TotalCount)> GetAllByUserIdAsync(
            string userId, int pageNumber, int pageSize,
            DateTimeOffset? fromDate, DateTimeOffset? toDate, string? readStatus);
        Task<int> UpdateAsync(int id, Domain.Entities.Notification.NotificationEventLog NotificationLog);
    }
}