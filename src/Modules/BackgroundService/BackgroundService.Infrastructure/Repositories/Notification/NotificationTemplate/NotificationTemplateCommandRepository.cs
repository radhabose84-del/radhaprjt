using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationTemplate
{
    public class NotificationTemplateCommandRepository : INotificationTemplateCommandRepository
    {
        private readonly NotificationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public NotificationTemplateCommandRepository(NotificationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(Domain.Entities.Notification.NotificationTemplate NotificationTemplate)
        {
            await _applicationDbContext.NotificationTemplate.AddAsync(NotificationTemplate);
            await _applicationDbContext.SaveChangesAsync();
            return NotificationTemplate.Id;
        }

        public async Task<int> DeleteAsync(int Id, Domain.Entities.Notification.NotificationTemplate NotificationTemplate)
        {
            var NotificationTemplateToDelete = await _applicationDbContext.NotificationTemplate.FirstOrDefaultAsync(u => u.Id == Id);
            if (NotificationTemplateToDelete is null)
            {
                return -1;
            }
            // Update the IsActive status to indicate deletion (or soft delete)
            NotificationTemplateToDelete.IsDeleted = NotificationTemplate.IsDeleted;
            // Save changes to the database 
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<int> UpdateAsync(int Id, Domain.Entities.Notification.NotificationTemplate NotificationTemplate)
        {
            var existingNotificationTemplate = await _applicationDbContext.NotificationTemplate.FirstOrDefaultAsync(u => u.Id == Id);
            if (existingNotificationTemplate is null)
            {
                return -1;
            }
            existingNotificationTemplate.NotificationTypeId = NotificationTemplate.NotificationTypeId;
            existingNotificationTemplate.NotificationConfigId = NotificationTemplate.NotificationConfigId;
            existingNotificationTemplate.SubjectTemplate = NotificationTemplate.SubjectTemplate;
            existingNotificationTemplate.BodyTemplate = NotificationTemplate.BodyTemplate;
            existingNotificationTemplate.HeaderTemplate = NotificationTemplate.HeaderTemplate;
            existingNotificationTemplate.FooterTemplate = NotificationTemplate.FooterTemplate;
            existingNotificationTemplate.LanguageCode = NotificationTemplate.LanguageCode;
            existingNotificationTemplate.IsActive = NotificationTemplate.IsActive;
            // Mark the entity as modified
            _applicationDbContext.NotificationTemplate.Update(existingNotificationTemplate);
            // Save changes to the database
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
        public async Task<bool> IsNameDuplicateAsync(int notificationConfigId, int notificationTypeId, string languageCode, int excludeId)
        {
            return await _applicationDbContext.NotificationTemplate
                .AnyAsync(cc => cc.NotificationTypeId == notificationTypeId && cc.NotificationConfigId == notificationConfigId && cc.LanguageCode == languageCode && cc.IsDeleted == 0 && cc.Id != excludeId); ;
        }
        public async Task<bool> ExistsByCodeAsync(int notificationConfigId, int notificationTypeId, string languageCode)
        {
            return await _applicationDbContext.NotificationTemplate
                .AnyAsync(cc => cc.NotificationTypeId == notificationTypeId && cc.NotificationConfigId == notificationConfigId && cc.LanguageCode == languageCode && cc.IsDeleted == 0);
        }
    }
}