using BackgroundService.Domain.Entities.Notification;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationEventRule
{
    public interface INotificationEventRuleCommand
    {
        Task<NotificationEventRule?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(NotificationEventRule entity);
        Task<bool> InsertAsync(NotificationEventRule entity);
        Task<bool> DeleteByHierarchyIdAsync (int hierarchyId);
        Task<bool> DeleteRangeAsync(List<NotificationEventRule> rules);
    }
}
