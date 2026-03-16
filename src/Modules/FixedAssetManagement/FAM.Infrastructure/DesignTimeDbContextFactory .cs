using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Contracts.Interfaces;
using FAM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using FAM.Infrastructure.Services;

namespace FAM.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Build configuration
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../BSOFT.Api"))
                .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                .Build();

          
            //var connectionString = ConnectionStringHelper.GetDefaultConnectionString(configuration);
            
          var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                                                .Replace("{SERVER}","192.168.1.126")
                                                .Replace("{USER_ID}","Developer")
                                                .Replace("{ENC_PASSWORD}", "Dev@#$456");

            optionsBuilder.UseSqlServer(connectionString);

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            IIPAddressService ipAddressService = new DesignTimeMockIPAddressService();
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new ApplicationDbContext(optionsBuilder.Options, ipAddressService, timeZoneService);
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
        public string GetOldUnitId() => string.Empty;
            public int? GetPartyId() => null;
    }
    }
}