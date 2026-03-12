using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationConfig;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig
{
    public class NotificationConfigCommandRepository : INotificationConfigCommandRepository
    {
        private readonly NotificationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public NotificationConfigCommandRepository(NotificationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }


        public async Task<int> CreateAsync(Domain.Entities.Notification.NotificationConfig notificationConfig)
        {
            notificationConfig.UnitId = _ipAddressService.GetUnitId() ?? 0;
            notificationConfig.NotificationEventType = null;
            //_applicationDbContext.Entry(notificationConfig);
            await _applicationDbContext.NotificationConfig.AddAsync(notificationConfig);
            await _applicationDbContext.SaveChangesAsync();
            return notificationConfig.Id;
        }

        public async Task<int> DeleteAsync(int Id, Domain.Entities.Notification.NotificationConfig notificationConfig)
        {
            var NotificationConfigToDelete = await _applicationDbContext.NotificationConfig.FirstOrDefaultAsync(u => u.Id == Id);
            if (NotificationConfigToDelete is null)
            {
                return -1;
            }
            // Update the IsActive status to indicate deletion (or soft delete)
            NotificationConfigToDelete.IsDeleted = notificationConfig.IsDeleted;
            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<int> UpdateAsync(int Id, Domain.Entities.Notification.NotificationConfig notificationConfig)
        {
            var existingNotificationConfig = await _applicationDbContext.NotificationConfig.FirstOrDefaultAsync(u => u.Id == Id);
            if (existingNotificationConfig is null)
            {
                return -1;
            }
            existingNotificationConfig.ModuleName = notificationConfig.ModuleName;
            existingNotificationConfig.NotificationEventTypeId = notificationConfig.NotificationEventTypeId;
            existingNotificationConfig.IsActive = notificationConfig.IsActive;
            // Mark the entity as modified
            _applicationDbContext.NotificationConfig.Update(existingNotificationConfig);
            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<bool> IsNameDuplicateAsync(string? name, int notificationEventTypeId, int excludeId)
        {
            return await _applicationDbContext.NotificationConfig
                .AnyAsync(cc => cc.ModuleName == name && cc.NotificationEventTypeId == notificationEventTypeId && cc.IsDeleted == 0 && cc.Id != excludeId);
        }       
        public async Task<bool> ExistsByCodeAsync(string? name, int notificationEventTypeId)
        {
            return await _applicationDbContext.NotificationConfig
                .AnyAsync(cc => cc.ModuleName == name && cc.NotificationEventTypeId == notificationEventTypeId && cc.IsDeleted == 0);
        }     
    }
}