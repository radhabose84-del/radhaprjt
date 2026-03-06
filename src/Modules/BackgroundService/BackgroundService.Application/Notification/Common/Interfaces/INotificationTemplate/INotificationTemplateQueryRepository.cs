
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetAllNotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Queries.GetNotificationTemplateAutoComplete;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate
{
    public interface INotificationTemplateQueryRepository
    {
        Task<(IEnumerable<dynamic>, int)> GetAllNotificationTemplateAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<NotificationTemplateDto> GetByIdAsync(int id);
        Task<List<NotificationTemplateAutoCompleteDto>> GetNotificationTemplateAutoCompleteAsync(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id);        
        Task<bool> NotFoundAsync(int Id );
    }
}