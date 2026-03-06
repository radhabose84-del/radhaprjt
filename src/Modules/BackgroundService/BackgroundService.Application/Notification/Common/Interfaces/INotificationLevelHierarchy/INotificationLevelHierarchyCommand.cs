using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationLevelHierarchy
{
    public interface INotificationLevelHierarchyCommand
    {
        Task<NotificationLevelHierarchy?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(NotificationLevelHierarchy entity);
        Task<bool> InsertAsync(NotificationLevelHierarchy entity);
        Task<(List<NotificationLevelHierarchy> Hierarchies, int TotalCount)> GetAllWithEventRuleAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<NotificationLevelHierarchy?> GetByIdWithEventRuleAsync(int id);
        Task<bool> DeleteAsync(NotificationLevelHierarchy entity);
        Task<bool> ExistsByCodeAsync(int configId, int targetTypeId, int targetId);
        Task<bool> ExistsByCodeExcludingIdAsync(int configId, int targetTypeId, int targetId, int excludeId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidation(int id);         
    }
}
