#nullable disable
using UserManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using UserManagement.Infrastructure.Services;
// using UserManagement.Infrastructure.Helpers;  // Ensure this is included if needed for IHttpContextAccessor

namespace UserManagement.Infrastructure
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

            /* var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                                                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                                                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? ""); */
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                                        .Replace("{SERVER}", "192.168.1.126")
                                                        .Replace("{USER_ID}", "developer")
                                                        .Replace("{ENC_PASSWORD}", "Dev@#$456");

            //   var connectionString = ConnectionStringHelper.GetDefaultConnectionString(configuration);

            optionsBuilder.UseSqlServer(connectionString);

            IIPAddressService ipAddressService = new DesignTimeMockIPAddressService();
            ITimeZoneService timeZoneService = new TimeZoneService();

            return new ApplicationDbContext(optionsBuilder.Options, ipAddressService, timeZoneService);  // Pass both dependencies
            //return new ApplicationDbContext(optionsBuilder.Options);  // Pass both dependencies
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