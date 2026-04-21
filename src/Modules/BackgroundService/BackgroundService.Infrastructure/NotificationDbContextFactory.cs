using Contracts.Interfaces;
using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using BackgroundService.Infrastructure.Services;

namespace BackgroundService.Infrastructure
{
    public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
    {
        public NotificationDbContext CreateDbContext(string[] args)
        {
             var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            // Build configuration
             IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

          var connectionString = (configuration.GetConnectionString("NotificationConnection") ?? string.Empty)
                                                .Replace("{SERVER}","192.168.1.126")
                                                .Replace("{USER_ID}","Developer")
                                                .Replace("{ENC_PASSWORD}", "Dev@#$456");


            optionsBuilder.UseSqlServer(connectionString);
            IIPAddressService ipAddressService = new DesignTimeMockIPAddressService();
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new NotificationDbContext(optionsBuilder.Options, ipAddressService,timeZoneService);
        }

        private sealed class DesignTimeMockIPAddressService : IIPAddressService
        {
            public string GetSystemIPAddress() => "127.0.0.1";
            public string GetUserIPAddress() => "127.0.0.1";
            public string GetUserAgent() => "design-time";
            public string GetCurrentUserId() => "0";
            public int    GetUserId() => 0;
            public string GetUserName() => "design-time";
            public string GetUserOS() => "Unknown";
            public string GetUserBrowserDetails(string userAgent) => "design-time";
            public int?   GetCompanyId() => null;
            public string GetGroupCode() => string.Empty;
            public int    GetEntityId() => 0;
            public int?   GetUnitId() => null;
            public int    GetUnitTypeId() => 0;
            public string GetUnitTypeName() => string.Empty;
            public string GetOldUnitId() => string.Empty;
            public int? GetPartyId() => null;
            public int? GetEmpId() => null;
            public int? GetDivisionId() => null;
        }
    }
}