
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail
{
    public interface INotificationDetailRepository
    {     
        Task<List<GetNotificationDetailDto>> GetAllByUserIdAsync(string userId);
        Task<int> UpdateAsync(int id, Domain.Entities.Notification.NotificationEventLog NotificationLog);
    }
}