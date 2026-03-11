using System.Threading.Tasks;
using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationWhatsAppGroup;
using BackgroundService.Infrastructure.Data.Notification;
using NotificationWhatsAppGroupEntity = BackgroundService.Core.Domain.Entities.Notifications.NotificationWhatsAppGroup;
using Microsoft.EntityFrameworkCore;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationWhatsAppGroup
{
    public class NotificationWhatsAppGroupCommandRepository : INotificationWhatsAppGroupCommand
    {
        private readonly NotificationDbContext _notificationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public NotificationWhatsAppGroupCommandRepository(
            NotificationDbContext notificationDbContext,
            IIPAddressService ipAddressService)
        {
            _notificationDbContext = notificationDbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(NotificationWhatsAppGroupEntity notificationWhatsAppGroup)
        {
            // Resolve UnitId from context
            notificationWhatsAppGroup.UnitId = _ipAddressService.GetUnitId() ?? 0;

            // Track + insert
            _notificationDbContext.Entry(notificationWhatsAppGroup);
            await _notificationDbContext.NotificationWhatsAppGroup.AddAsync(notificationWhatsAppGroup);
            await _notificationDbContext.SaveChangesAsync();

            return notificationWhatsAppGroup.Id;
        }

        public async Task<bool> UpdateAsync(NotificationWhatsAppGroupEntity notificationWhatsAppGroup)
        {
            var existing = await _notificationDbContext.NotificationWhatsAppGroup
                .FirstOrDefaultAsync(x => x.Id == notificationWhatsAppGroup.Id);

            if (existing == null)
                return false;

            // Update allowed fields
            existing.DepartmentId = notificationWhatsAppGroup.DepartmentId;
            existing.GroupName = notificationWhatsAppGroup.GroupName;
            existing.ApiKey = notificationWhatsAppGroup.ApiKey;
            existing.IsActive = notificationWhatsAppGroup.IsActive;

            // UnitId stays same; audit fields handled in SaveChanges
            await _notificationDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id, NotificationWhatsAppGroupEntity notificationWhatsAppGroup)
        {
            var existing = await _notificationDbContext.NotificationWhatsAppGroup
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
                return false;

            // soft delete
            existing.IsDeleted = notificationWhatsAppGroup.IsDeleted;

            return await _notificationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByNameAsync(string groupName, int departmentId, int? excludeId = null, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var trimmedName = groupName.Trim();

            var query = _notificationDbContext.NotificationWhatsAppGroup
                .AsNoTracking()
                .Where(x =>
                    x.UnitId == unitId &&
                    x.DepartmentId == departmentId &&
                    x.GroupName == trimmedName &&
                    x.IsDeleted == IsDelete.NotDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            return await query.AnyAsync(ct);
        }
    }
}
