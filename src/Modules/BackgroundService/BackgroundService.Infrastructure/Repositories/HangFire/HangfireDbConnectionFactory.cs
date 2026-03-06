using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces;
using Microsoft.Data.SqlClient;

namespace BackgroundService.Infrastructure.Repositories.HangFire
{
    public class HangfireDbConnectionFactory : IHangfireDbConnectionFactory
    {
        private readonly string _connectionString;

         public HangfireDbConnectionFactory(string connectionString)
         {
             _connectionString = connectionString;
         }

         public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}