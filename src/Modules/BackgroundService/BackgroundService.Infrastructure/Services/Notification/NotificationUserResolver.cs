
using System.Data;
using BackgroundService.Application.DTO;
using BackgroundService.Application.Interfaces.Notification;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Services.Notification
{
    public class NotificationUserResolver : INotificationUserResolver
    {
        private readonly IDbConnection _dbConnection;

        public NotificationUserResolver([FromKeyedServices("Notification")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<NotificationTargetDto>> GetNotificationTargetsAsync(int unitId, string moduleName, int eventTypeId,string email,string ccMail,string mobile)
        {
             var parameters = new DynamicParameters();
                parameters.Add("@UnitId", unitId);
                parameters.Add("@ModuleName", moduleName);
                parameters.Add("@EventType", eventTypeId);
                parameters.Add("@Email",   email  ?? string.Empty);
                parameters.Add("@CCEmail", ccMail ?? string.Empty);   
                parameters.Add("@Mobile",  mobile ?? string.Empty);                

            var result = (await _dbConnection.QueryAsync<NotificationTargetDto>(
                "WorkFlow_GetUserId", 
                parameters,
                commandType: CommandType.StoredProcedure
            )).ToList();
            return result;
        }
    }

}
