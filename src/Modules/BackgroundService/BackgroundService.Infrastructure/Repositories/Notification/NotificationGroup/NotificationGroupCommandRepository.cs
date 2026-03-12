using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationGroup;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationGroup
{
    public class NotificationGroupCommandRepository : INotificationGroupCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public NotificationGroupCommandRepository(NotificationDbContext notificationDbContext, IIPAddressService ipAddressService)
        {
            _notificationDbContext = notificationDbContext;
            _ipAddressService = ipAddressService;
        }
        public async Task<int> CreateAsync(Domain.Entities.Notification.NotificationGroup notificationGroup)
        {
            notificationGroup.UnitId = _ipAddressService.GetUnitId() ?? 0;
            _notificationDbContext.Entry(notificationGroup);
            await _notificationDbContext.NotificationGroup.AddAsync(notificationGroup);
            await _notificationDbContext.SaveChangesAsync();
            return notificationGroup.Id;
        }

        public async Task<bool> DeleteAsync(int id, Domain.Entities.Notification.NotificationGroup notificationGroup)
        {
             var NotificationDelete = await _notificationDbContext.NotificationGroup.FirstOrDefaultAsync(u => u.Id == id);
            if (NotificationDelete != null)
            {
                NotificationDelete.IsDeleted = notificationGroup.IsDeleted;
                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            return false; 
        }

        public async Task<bool> UpdateAsync(Domain.Entities.Notification.NotificationGroup notificationGroup)
        {
             var existingNotification = await _notificationDbContext.NotificationGroup
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == notificationGroup.Id);
            
            if (existingNotification != null)
            {
                existingNotification.GroupName = notificationGroup.GroupName;
                existingNotification.IsActive = notificationGroup.IsActive;
                _notificationDbContext.NotificationGroup.Update(existingNotification);

                return await _notificationDbContext.SaveChangesAsync() >0;
            }
            
            return false; 
        }
    }
}