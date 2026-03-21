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
            const string query = @" SELECT L.Id,NC.ModuleName,MM.Code  EventType,MM1.Code TargetType,MM2.Code ChannelName,
                    ActionStatus, MM4.Code ReadStatus,MM4.Id ReadStatusId, MessageText, Timestamp, L.CreatedBy,L.CreatedDate, L.CreatedByName, L.CreatedIP, SendTo,
                    L.Value
                    FROM AppNotification.NotificationEventLog L
                    INNER JOIN  AppNotification.NotificationEventRule NR on NR.Id=L.NotificationLevelRuleId
                    INNER JOIN  AppNotification.NotificationLevelHierarchy NH  on NR.NotificationLevelHierarchyId=NH.Id 
                    INNER JOIN AppData.MiscMaster MM2 on MM2.Id=NR.NotificationChannelId  
                    INNER JOIN AppNotification.NotificationTemplate NT on NT.ID=NR.TemplateId  
                    INNER JOIN AppNotification.NotificationConfig NC on NC.id=NH.NotificationConfigId  
                    INNER JOIN AppData.MiscMaster MM on MM.id=NC.NotificationEventTypeId  
                    INNER JOIN AppData.MiscMaster MM1 on MM1.id=NH.TargetTypeId  
                    INNER JOIN AppData.MiscMaster MM4 on MM4.id=L.ReadStatusId
                    where L.SendTo = @userId AND L.IsDeleted = 0 and L.UnitId = @UnitId
                    ORDER BY L.Timestamp DESC ";
      
            var notifications = await _dbConnection.QueryAsync<GetNotificationDetailDto>(query, new { userId,UnitId });
            return notifications.ToList();
        }

        public async Task<int> UpdateAsync(int id, NotificationEventLog NotificationLog)
        {
            var existingNotificationDetail = await _applicationDbContext.NotificationEventLog.FirstOrDefaultAsync(u => u.Id == id);
            if (existingNotificationDetail is null)
            {
                return -1;
            }

            // Resolve MiscMaster ID dynamically
            var readMisc = await _appDataMiscLookup.GetMiscMasterByNameAsync(NotificationEnum.NotificationReadStatus, NotificationEnum.Read);
            existingNotificationDetail.ReadStatusId = readMisc?.Id ?? 0;

            _applicationDbContext.NotificationEventLog.Update(existingNotificationDetail);
            await _applicationDbContext.SaveChangesAsync();
            return 1;
        }
    }
}