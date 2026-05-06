using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces.INotificationDetail;
using BackgroundService.Application.Notification.GetNotificationDetail.GetNotificationDetailById;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Infrastructure.Data.Notification;
using Contracts.Events.Notifications;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Notification.NotificationDetail
{
    public class NotificationDetailRepository : INotificationDetailRepository
    {
        private readonly NotificationDbContext _applicationDbContext;
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly IAppDataMiscMasterLookup _appDataMiscLookup;

        public NotificationDetailRepository([FromKeyedServices("Notification")] IDbConnection dbConnection, NotificationDbContext applicationDbContext, IIPAddressService iPAddressService, IAppDataMiscMasterLookup appDataMiscLookup)
        {
            _dbConnection = dbConnection;
            _applicationDbContext = applicationDbContext;
            _ipAddressService = iPAddressService;
            _appDataMiscLookup = appDataMiscLookup;
        }

        public async Task<List<GetNotificationDetailDto>> GetAllByUserIdAsync(string userId)
        {
            var UnitId = _ipAddressService.GetUnitId() ?? 0;
            const string query = @"
                    SELECT L.Id, NC.ModuleName, MM.Code AS EventType, MM1.Code AS TargetType, MM2.Code AS ChannelName,
                    ActionStatus, MM4.Code AS ReadStatus, MM4.Id AS ReadStatusId, MessageText, Timestamp,
                    L.CreatedBy, L.CreatedDate, L.CreatedByName, L.CreatedIP, SendTo, L.Value
                    FROM AppNotification.NotificationEventLog L
                    INNER JOIN AppNotification.NotificationEventRule NR ON NR.Id = L.NotificationLevelRuleId
                    INNER JOIN AppNotification.NotificationLevelHierarchy NH ON NR.NotificationLevelHierarchyId = NH.Id
                    INNER JOIN AppData.MiscMaster MM2 ON MM2.Id = NR.NotificationChannelId
                    INNER JOIN AppNotification.NotificationTemplate NT ON NT.Id = NR.TemplateId
                    INNER JOIN AppNotification.NotificationConfig NC ON NC.Id = NH.NotificationConfigId
                    INNER JOIN AppData.MiscMaster MM ON MM.Id = NC.NotificationEventTypeId
                    INNER JOIN AppData.MiscMaster MM1 ON MM1.Id = NH.TargetTypeId
                    INNER JOIN AppData.MiscMaster MM4 ON MM4.Id = L.ReadStatusId
                    WHERE CAST(L.SendTo AS NVARCHAR(500)) = @userId AND L.IsDeleted = 0 AND L.UnitId = @UnitId
                    ORDER BY L.Id desc ";

            var notifications = await _dbConnection.QueryAsync<GetNotificationDetailDto>(query, new { userId, UnitId });
            return notifications.ToList();
        }

        public async Task<int> UpdateAsync(int id, NotificationEventLog NotificationLog)
        {
            var existingNotificationDetail = await _applicationDbContext.NotificationEventLog.FirstOrDefaultAsync(u => u.Id == id);
            if (existingNotificationDetail is null)
            {
                return -1;
            }

            // Resolve MiscMaster ID dynamically. ReadStatusId is a FK to AppData.MiscMaster —
            // a missing 'Read' entry under 'NotificationReadStatus' indicates a configuration
            // problem, not something to silently fall back from (Id = 0 would violate the FK).
            var readMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationReadStatus, NotificationEnum.Read);
            if (readMisc is null)
            {
                throw new InvalidOperationException(
                    $"MiscMaster entry not found for type '{NotificationEnum.NotificationReadStatus}', " +
                    $"code '{NotificationEnum.Read}'. Cannot mark notification as read.");
            }
            existingNotificationDetail.ReadStatusId = readMisc.Id;

            _applicationDbContext.NotificationEventLog.Update(existingNotificationDetail);
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
    }
}