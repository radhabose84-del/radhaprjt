namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig
{
    public interface INotificationConfigCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.Notification.NotificationConfig notificationConfig);
        Task<int> UpdateAsync(int id, Domain.Entities.Notification.NotificationConfig notificationConfig);
        Task<int> DeleteAsync(int id, Domain.Entities.Notification.NotificationConfig notificationConfig);
        Task<bool> IsNameDuplicateAsync(string? name, int notificationEventTypeId, int excludeId);     
         Task<bool> ExistsByCodeAsync(string? name, int notificationEventTypeId);           
    }
}