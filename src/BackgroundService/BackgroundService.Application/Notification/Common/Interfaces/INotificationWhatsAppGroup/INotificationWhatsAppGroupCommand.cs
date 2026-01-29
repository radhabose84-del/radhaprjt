using System.Threading.Tasks;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;

namespace BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup
{
    public interface INotificationWhatsAppGroupCommand
    {
        Task<int> CreateAsync(NotificationWhatsAppGroupEntity notificationWhatsAppGroup);
        Task<bool> UpdateAsync(NotificationWhatsAppGroupEntity notificationWhatsAppGroup);
        Task<bool> DeleteAsync(int id, NotificationWhatsAppGroupEntity notificationWhatsAppGroup);
        Task<bool> ExistsByNameAsync(string groupName, int departmentId, int? excludeId = null, CancellationToken ct = default);
                               
    }
}
