using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Common.Notification.Interfaces;
using Microsoft.Data.SqlClient;

namespace BackgroundService.Infrastructure.Repositories.Notification
{
    public class NotificationDbConnectionFactory : INotificationDbConnectionFactory
    {
        private readonly string _connectionString;

         public NotificationDbConnectionFactory(string connectionString)
         {
             _connectionString = connectionString;
         }
        
         public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
        
    }
}