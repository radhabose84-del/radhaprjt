
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetAllNotificationConfig;
using BackgroundService.Application.Notification.NotificationConfig.Queries.GetNotificationConfigAutoComplete;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig
{
    public interface INotificationConfigQueryRepository
    {
        Task<(IEnumerable<dynamic>, int)> GetAllNotificationConfigAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<NotificationConfigDto> GetByIdAsync(int id);
        Task<List<NotificationConfigAutoCompleteDto>> GetNotificationConfigAutoCompleteAsync(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id);        
        Task<bool> NotFoundAsync(int Id );
    }
}