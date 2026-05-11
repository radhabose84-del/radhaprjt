using System.Data;
using BackgroundService.Application.Interfaces.Notification;
using Contracts.Interfaces;
using BackgroundService.Domain.Entities.Notification;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.Notification
{
    public class NotificationLogger : INotificationLogger
    {
        private readonly IDbConnection _dbConnection;         

        public NotificationLogger([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;            
        }
        public async Task<int> LogAsync(NotificationEventLog log)
        {
            
            const string sql = @"
            INSERT INTO AppNotification.NotificationEventLog
            (NotificationLevelRuleId, UnitId, ChannelId, NotificationStatusId, MessageText, ActionStatus, ReadStatusId, Timestamp, SendTo,
            IsActive, IsDeleted, CreatedBy, CreatedDate, CreatedByName, CreatedIP, Value, ModuleTransactionId, ModuleTypeName)
            OUTPUT INSERTED.Id
            VALUES (@NotificationLevelRuleId, @UnitId, @ChannelId, @NotificationStatusId, @MessageText, @ActionStatus, @ReadStatusId, @Timestamp, @SendTo,
                    1, 0, @CreatedBy, SYSDATETIMEOFFSET(), @CreatedByName, @CreatedIP, @Value, @ModuleTransactionId, @ModuleTypeName);";

                var id = await _dbConnection.ExecuteScalarAsync<int>(sql, new
                {
                    log.NotificationLevelRuleId ,
                    log.UnitId,
                    log.ChannelId,
                    log.NotificationStatusId,
                    log.MessageText,
                    log.ActionStatus,
                    log.ReadStatusId,
                    log.Timestamp,
                    log.SendTo,
                    log.CreatedBy,
                    log.CreatedByName,
                    log.CreatedIP,
                    log.Value,
                    log.ModuleTransactionId,
                    log.ModuleTypeName
                });
                return id;
        }   
    }
}