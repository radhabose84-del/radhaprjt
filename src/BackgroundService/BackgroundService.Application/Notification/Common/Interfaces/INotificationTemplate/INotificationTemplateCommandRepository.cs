namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate
{
    public interface INotificationTemplateCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.Notification.NotificationTemplate NotificationTemplate);
        Task<int> UpdateAsync(int id, Domain.Entities.Notification.NotificationTemplate NotificationTemplate);
        Task<int> DeleteAsync(int id, Domain.Entities.Notification.NotificationTemplate NotificationTemplate);
        Task<bool> IsNameDuplicateAsync(int notificationConfigId, int notificationTypeId, string languageCode, int excludeId);
        Task<bool> ExistsByCodeAsync( int notificationConfigId,int notificationTypeId, string languageCode);
                
    }
}