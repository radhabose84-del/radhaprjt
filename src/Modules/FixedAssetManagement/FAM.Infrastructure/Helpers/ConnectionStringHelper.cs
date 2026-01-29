using Microsoft.Extensions.Configuration;
using System;

namespace FAM.Infrastructure.Helpers
{
    public static class ConnectionStringHelper
    {
        
        public static string GetDefaultConnectionString(IConfiguration configuration)
        {
           
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");
            return connectionString;
        }

        public static string GetHangfireConnectionString(IConfiguration configuration)
        {
          
            var connectionString = configuration.GetConnectionString("HangfireConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");            
            return connectionString;
        }
    }
}
