using BackgroundService.Application.DTO;

namespace BackgroundService.Application.Interfaces.Notification
{
    public interface INotificationUserResolver
    {
        Task<List<NotificationTargetDto>> GetNotificationTargetsAsync(int unitId, string moduleName, int eventTypeId, string email, string ccMail, string mobile, string userId);
    }
}